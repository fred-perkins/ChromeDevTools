using System;
using System.IO;

namespace MasterDevs.ChromeDevTools
{
    public interface IChromeProcessFactory
    {
        IChromeProcess CreateRemote(Uri url);

        IChromeProcess CreateLocal(int port, bool headless = true, DirectoryInfo userDataDirectory = default);
    }
}