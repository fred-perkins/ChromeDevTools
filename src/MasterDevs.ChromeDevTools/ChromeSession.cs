using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MasterDevs.ChromeDevTools.Serialization;
using Newtonsoft.Json;

namespace MasterDevs.ChromeDevTools
{
    public class ChromeSession : IChromeSession
    {
        private readonly string endpointUri;
        private readonly ConcurrentDictionary<string, ConcurrentBag<Delegate>> eventHandlers = new ConcurrentDictionary<string, ConcurrentBag<Delegate>>();
        private readonly ConcurrentDictionary<long, ManualResetEventSlim> requestWaitHandles = new ConcurrentDictionary<long, ManualResetEventSlim>();
        private readonly ConcurrentDictionary<long, ICommandResponse> responses = new ConcurrentDictionary<long, ICommandResponse>();
        private readonly ICommandFactory commandFactory;
        private readonly IEventFactory eventFactory;
        private readonly ICommandResponseFactory responseFactory;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly List<ArraySegment<byte>> incomingMessageParts = new List<ArraySegment<byte>>();
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new MessageContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        private ClientWebSocket clientWebSocket;
        private Task recieveHandlerTask;

        public event Action<string> UnknownMessageReceived;

        public event Action<byte[]> UnknownDataReceived;

        public ChromeSession(string endpointUri, ICommandFactory commandFactory, ICommandResponseFactory responseFactory, IEventFactory eventFactory)
        {
            this.endpointUri = endpointUri;
            this.commandFactory = commandFactory;
            this.responseFactory = responseFactory;
            this.eventFactory = eventFactory;
        }

        public void Dispose()
        {
            if (clientWebSocket?.State == WebSocketState.Open)
            {
                clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Socket no longer required", CancellationToken.None).Wait();
            }

            clientWebSocket?.Dispose();
            recieveHandlerTask?.Dispose();
        }

        private async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if(clientWebSocket?.State == WebSocketState.Open)
            {
                return;
            }

            clientWebSocket = new ClientWebSocket();
            clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(100);

            await clientWebSocket.ConnectAsync(new Uri(endpointUri), cancellationToken);

            StartRecieveHandler();
        }

        private void StartRecieveHandler()
        {
            var token = cancellationTokenSource.Token;
            recieveHandlerTask = Task.Run(async () =>
            {
                var buffer = WebSocket.CreateServerBuffer(4096);
                while(clientWebSocket?.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await clientWebSocket.ReceiveAsync(buffer, cancellationTokenSource.Token);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    if(result.EndOfMessage)
                    {
                        byte[] data;
                        if(incomingMessageParts.Count > 0)
                        {
                            data = incomingMessageParts.SelectMany(a => a.Array).ToArray();
                        }
                        else
                        {
                            data = buffer.Array;
                        }
                        ProcessRecievedMessage(data);
                    }
                    else
                    {
                        incomingMessageParts.Add(buffer);
                    }
                }
            }, token);
        }

        public Task<ICommandResponse> SendAsync<T>(CancellationToken cancellationToken)
        {
            var command = commandFactory.Create<T>();
            return SendCommand(command, cancellationToken);
        }

        public Task<CommandResponse<T>> SendAsync<T>(ICommand<T> parameter, CancellationToken cancellationToken)
        {
            var command = commandFactory.Create(parameter);
            var task = SendCommand(command, cancellationToken);
            return CastTaskResult<ICommandResponse, CommandResponse<T>>(task);
        }

        private Task<TDerived> CastTaskResult<TBase, TDerived>(Task<TBase> task) where TDerived: TBase
        {
            var tcs = new TaskCompletionSource<TDerived>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.SetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    tcs.SetResult((TDerived)t.Result);
                }
            });
            return tcs.Task;
        }

        public void Subscribe<T>(Action<T> handler)
            where T : class
        {
            var handlerName = typeof(T).FullName;

            var handlerCollection = eventHandlers.GetOrAdd(
                handlerName, 
                _ => new ConcurrentBag<Delegate>(new[] { handler }));

            handlerCollection.Add(handler);
        }

        private void HandleGenericEvent(IEvent evnt)
        {
            var type = evnt?.GetType()?.GetGenericArguments()?.FirstOrDefault();
            if (type is null)
            {
                return;
            }

            if (eventHandlers.TryGetValue(type.FullName, out var handlers))
            {
                //Force enumeration of the handlers so that if the enumeration changes we don't throw an exception - as this is concurrent.
                foreach (var handler in handlers.ToArray())
                {
                    ExecuteHandler(handler, evnt);
                }
            }
        }

        private void ExecuteHandler(Delegate handler, IEvent evnt)
        {
            var genericEvent = evnt.GetType().GetGenericTypeDefinition();
            if (genericEvent == typeof(Event<>))
            {
                object value = genericEvent.GetProperty("Params").GetValue(evnt);
                handler.DynamicInvoke(value);
            }
            else
            {
                handler.DynamicInvoke(evnt);
            }
        }

        private void HandleResponse(ICommandResponse response)
        {
            if (response is null)
            {
                return;
            }

            if (requestWaitHandles.TryGetValue(response.Id, out ManualResetEventSlim requestMre))
            {
                responses.AddOrUpdate(response.Id, id => response, (key, value) => response);
                requestMre.Set();
            }
            else
            {
                // in the case of an error, we don't always get the request Id back :(
                // if there is only one pending requests, we know what to do ... otherwise
                if (requestWaitHandles.Any())
                {
                    var requestId = requestWaitHandles.Keys.First();
                    requestWaitHandles.TryGetValue(requestId, out requestMre);
                    responses.AddOrUpdate(requestId, id => response, (key, value) => response);
                    requestMre.Set();
                }
            }
        }

        private Task<ICommandResponse> SendCommand(Command command, CancellationToken cancellationToken)
        {
            var requestString = JsonConvert.SerializeObject(command, settings);

            var requestResetEvent = requestWaitHandles.GetOrAdd(command.Id, new ManualResetEventSlim(false));

            return Task.Run(async () =>
            {
                await InitAsync();

                var bytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(requestString));
                try
                {
                    await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
                }
                catch(Exception e)
                {
                    Debug.WriteLine($"PANIC: {e.Message}");
                }
               
                requestResetEvent.Wait(cancellationToken);
                responses.TryRemove(command.Id, out ICommandResponse response);
                requestWaitHandles.TryRemove(command.Id, out requestResetEvent);

                return response;
            });
        }

        private bool TryGetCommandResponse(byte[] data, out ICommandResponse response)
        {
            response = responseFactory.Create(data);
            return null != response;
        }

        private bool TryGetCommandResponse(string message, out ICommandResponse response)
        {
            response = responseFactory.Create(message);
            return null != response;
        }

        private bool TryGetEvent(byte[] data, out IEvent evnt)
        {
            evnt = eventFactory.Create(data);
            return null != evnt;
        }

        private bool TryGetEvent(string message, out IEvent evnt)
        {
            evnt = eventFactory.Create(message);
            return null != evnt;
        }

        private void ProcessRecievedMessage(byte[] data)
        {
            if (TryGetCommandResponse(data, out ICommandResponse response))
            {
                HandleResponse(response);
                return;
            }

            if (TryGetEvent(data, out IEvent evnt))
            {
                HandleGenericEvent(evnt);
                return;
            }

            UnknownDataReceived?.Invoke(data);
        }

        private void ProcessMessage(string message)
        {
            if (TryGetCommandResponse(message, out ICommandResponse response))
            {
                HandleResponse(response);
                return;
            }

            if (TryGetEvent(message, out IEvent evnt))
            {
                HandleGenericEvent(evnt);
                return;
            }

            UnknownMessageReceived?.Invoke(message);
        }
    }
}