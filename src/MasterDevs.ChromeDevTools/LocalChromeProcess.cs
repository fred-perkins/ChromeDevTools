using System;
using System.Diagnostics;
using System.Threading;

namespace MasterDevs.ChromeDevTools
{
    public class LocalChromeProcess : RemoteChromeProcess
    {
        private readonly IUserDirectoryManager userDirectoryManager;

        public LocalChromeProcess(Uri remoteDebuggingUri, Process process, IUserDirectoryManager userDirectoryManager)
            : base(remoteDebuggingUri)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            this.userDirectoryManager = userDirectoryManager ?? throw new ArgumentNullException(nameof(userDirectoryManager));
        }

        public Action DisposeUserDirectory { get; set; }

        public Process Process { get; set; }

        public override void Dispose()
        {
            Process.Kill();
            Process.Dispose();
            //Wait for the process to actually dispose and stop modifying files.
            Thread.Sleep((int)TimeSpan.FromSeconds(2).TotalMilliseconds);
            userDirectoryManager.Dispose();

            base.Dispose();
        }
    }
}