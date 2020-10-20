using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MasterDevs.ChromeDevTools
{
    public class ChromeProcessFactory : IChromeProcessFactory
    {
        private const string HeadlessArguments = "--headless --disable-gpu";

        public IUserDirectoryManager UserDirectoryManager { get; }

        public string ChromePath { get; }

        public ChromeProcessFactory(IUserDirectoryManager userDirectoryManager, string chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")
        {
            UserDirectoryManager = userDirectoryManager;
            ChromePath = chromePath;
        }

        public IChromeProcess CreateLocal(int port, bool headless = true, DirectoryInfo userDataDirectory = default)
        {
            userDataDirectory ??= UserDirectoryManager.GetUserDirectory();

            var remoteDebuggingArg = $"--remote-debugging-port={port}";
            var userDirectoryArg = $"--user-data-dir=\"{userDataDirectory.FullName}\"";
            var chromeProcessArgs = new List<string>
            {
                remoteDebuggingArg,
                userDirectoryArg,
                "--bwsi",
                "--no-first-run"
            };

            if (headless)
            {
                chromeProcessArgs.Add(HeadlessArguments);
            }

            var processStartInfo = new ProcessStartInfo(ChromePath, string.Join(" ", chromeProcessArgs));
            var chromeProcess = Process.Start(processStartInfo);

            string remoteDebuggingUrl = "http://localhost:" + port;

            return new LocalChromeProcess(
                new Uri(remoteDebuggingUrl), 
                chromeProcess, 
                UserDirectoryManager);
        }

        public IChromeProcess CreateRemote(Uri url)
        {
            return new RemoteChromeProcess(url);
        }
    }
}