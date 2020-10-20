using System.Threading.Tasks;

namespace MasterDevs.ChromeDevTools
{
    public static class ChromeProcessExtensions
    {
        public static Task<ChromeSessionInfo[]> GetSessionInfo(this IChromeProcess process)
            => process.GetJsonAsync<ChromeSessionInfo[]>("/json");

        public static Task<ChromeSessionInfo> StartNewSession(this IChromeProcess process)
            => process.GetJsonAsync<ChromeSessionInfo>("/json/new");
    }
}