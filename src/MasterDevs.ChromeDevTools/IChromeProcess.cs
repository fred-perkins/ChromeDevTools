using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MasterDevs.ChromeDevTools
{
    public interface IChromeProcess : IDisposable
    {
        Uri RemoteDebuggingUri { get; }

        Task<ChromeSessionInfo[]> GetSessionInfo();

        Task<ChromeSessionInfo> StartNewSession();

        /// <summary>
        /// Invokes a local endpoint on the chrome debugging protocol.
        /// For example this could be used to get Protocol information from "/json/protocol/"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<JToken> GetJsonAsync(string path);

        /// <summary>
        /// Invokes a local endpoint on the chrome debugging protocol.
        /// For example this could be used to get Protocol information from "/json/protocol/"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<T> GetJsonAsync<T>(string path, JsonSerializerSettings jsonSerializerSettings = default);
    }
}