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
        private readonly HttpClient http;
        private JsonSerializerSettings defaultSettings = new JsonSerializerSettings();

        public RemoteChromeProcess(string remoteDebuggingUri)
            : this(new Uri(remoteDebuggingUri))
        {

        }

        public RemoteChromeProcess(Uri remoteDebuggingUri)
        {
            RemoteDebuggingUri = remoteDebuggingUri;

            http = new HttpClient
            {
                BaseAddress = RemoteDebuggingUri
            };
        }

        public Uri RemoteDebuggingUri { get; }

        public virtual void Dispose()
        {
            http.Dispose();
        }

        public async Task<ChromeSessionInfo[]> GetSessionInfo()
        {
            string json = await http.GetStringAsync("/json");
            return JsonConvert.DeserializeObject<ChromeSessionInfo[]>(json);
        }

        public async Task<ChromeSessionInfo> StartNewSession()
        {
            string json = await http.GetStringAsync("/json/new");
            return JsonConvert.DeserializeObject<ChromeSessionInfo>(json);
        }

        public async Task<JToken> GetJsonAsync(string path)
        {
            Stream jsonStream = await http.GetStreamAsync(path);

            using StreamReader streamReader = new StreamReader(jsonStream);
            using JsonReader reader = new JsonTextReader(streamReader);

            return await JToken.ReadFromAsync(reader);
        }

        public async Task<T> GetJsonAsync<T>(string path, JsonSerializerSettings jsonSerializerSettings = default)
        {
            string json = await http.GetStringAsync(path);
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings ?? defaultSettings);
        }
    }
}