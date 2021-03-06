﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Browser;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;

namespace MasterDevs.ChromeDevTools.Sample
{
    //SEE https://chromedevtools.github.io/devtools-protocol/
    internal class Program
    {
        const int ViewPortWidth = 800;
        const int ViewPortHeight = 600;

        static async Task Main(string[] args)
        {
            // synchronization
            var screenshotDone = new ManualResetEventSlim();

            // STEP 1 - Run Chrome
            var chromeProcessFactory = new ChromeProcessFactory(new RandomUserDirectoryManager());
            using (var chromeProcess = chromeProcessFactory.CreateLocal(9222, false))
            {
                // STEP 2 - Create a debugging session
                var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();
                var chromeSessionFactory = new ChromeSessionFactory();
                var chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);

                var response = await chromeSession.SendAsync(new GetVersionCommand());
                Console.WriteLine($"{response.Result.Product} using protocol version : {response.Result.ProtocolVersion}");

                // STEP 3 - Send a command
                await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
                {
                    Width = ViewPortWidth,
                    Height = ViewPortHeight,
                    Scale = 1
                });

                var navigateResponse = await chromeSession.SendAsync(new NavigateCommand
                {
                    Url = "http://www.google.com"
                });
                Console.WriteLine("NavigateResponse: " + navigateResponse.Id);

                // STEP 4 - Register for events (in this case, "Page" domain events)
                // send an command to tell chrome to send us all Page events
                // but we only subscribe to certain events in this session
                var pageEnableResult = await chromeSession.SendAsync<Protocol.Chrome.Page.EnableCommand>();
                Console.WriteLine("PageEnable: " + pageEnableResult.Id);

                chromeSession.Subscribe<LoadEventFiredEvent>(loadEventFired =>
                {
                // we cannot block in event handler, hence the task
                Task.Run(async () =>
                    {
                        Console.WriteLine("LoadEventFiredEvent: " + loadEventFired.Timestamp);

                        var documentNodeId = (await chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
                        var bodyNodeId =
                            (await chromeSession.SendAsync(new QuerySelectorCommand
                            {
                                NodeId = documentNodeId,
                                Selector = "body"
                            })).Result.NodeId;
                        var height = (await chromeSession.SendAsync(new GetBoxModelCommand { NodeId = bodyNodeId })).Result.Model.Height;

                        await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
                        {
                            Width = ViewPortWidth,
                            Height = height,
                            Scale = 1
                        });

                        Console.WriteLine("Taking screenshot");
                        var screenshot = await chromeSession.SendAsync(new CaptureScreenshotCommand { Format = "png" });

                        var data = Convert.FromBase64String(screenshot.Result.Data);
                        File.WriteAllBytes("output.png", data);
                        Console.WriteLine("Screenshot stored");

                    // tell the main thread we are done
                    screenshotDone.Set();
                    });
                });

                // wait for screenshoting thread to (start and) finish
                screenshotDone.Wait();

                Console.WriteLine("Exiting ..");
            }
        }
    }
}