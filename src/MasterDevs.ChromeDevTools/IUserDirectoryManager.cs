using System;
using System.IO;

namespace MasterDevs.ChromeDevTools
{
    public interface IUserDirectoryManager : IDisposable
    {
        DirectoryInfo GetUserDirectory();
    }
}