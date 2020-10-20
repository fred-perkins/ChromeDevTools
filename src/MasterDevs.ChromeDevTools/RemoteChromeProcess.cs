using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MasterDevs.ChromeDevTools
{
    public class RemoteChromeProcess : IChromeProcess
    {
        private static readonly JsonSerializerSettings defaultSettings = new JsonSerializerSettings();

        protected HttpClient Http { get; }

        public Uri RemoteDebuggingUri { get; }

        public RemoteChromeProcess(string remoteDebuggingUri)
            : this(new Uri(remoteDebuggingUri)) { }

        public RemoteChromeProcess(Uri remoteDebuggingUri)
        {
            RemoteDebuggingUri = remoteDebuggingUri;

            Http = new HttpClient
            {
                BaseAddress = RemoteDebuggingUri
            };
        }

        public virtual void Dispose()
        {
            Http.Dispose();
        }

        public async Task<JToken> GetJsonAsync(string path)
        {
            Stream jsonStream = await Http.GetStreamAsync(path);

            using StreamReader streamReader = new StreamReader(jsonStream);
            using JsonReader reader = new JsonTextReader(streamReader);

            return await JToken.ReadFromAsync(reader);
        }

        public async Task<T> GetJsonAsync<T>(string path, JsonSerializerSettings jsonSerializerSettings = default)
        {
            string json = await Http.GetStringAsync(path);
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings ?? defaultSettings);
        }
    }
}