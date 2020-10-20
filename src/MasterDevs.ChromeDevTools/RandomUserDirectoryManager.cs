using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MasterDevs.ChromeDevTools
{
    public class RandomUserDirectoryManager : IUserDirectoryManager
    {
        private DirectoryInfo currentDirectory;

        public DirectoryInfo GetUserDirectory()
        {
            if(currentDirectory == null)
            {
                currentDirectory = CreateRandomDirectory();
                AssignPermissions(currentDirectory);
            }
            
            return currentDirectory;
        }

        private static void AssignPermissions(DirectoryInfo dir)
        {
            DirectorySecurity security = new DirectorySecurity();
            var currentUser = WindowsIdentity.GetCurrent();
            security.AddAccessRule(new FileSystemAccessRule(currentUser.Name, FileSystemRights.FullControl, AccessControlType.Allow));
            dir.SetAccessControl(security);
        }

        public void Dispose()
        {
            currentDirectory.Delete(true);
        }

        private static DirectoryInfo CreateRandomDirectory()
        {
            return Directory.CreateDirectory(
                Path.Combine(
                    Path.GetTempPath(),
                    Path.GetRandomFileName()));
        }
    }
}