using System.Threading.Tasks;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    internal class Program
    {
        private const string TargetFolder = "OutputProtocol";

        public static async Task Main(string[] args)
        {
            var chromeProcessFactory = new ChromeProcessFactory(new RandomUserDirectoryManager());

            ProtocolDefinition currentProtocol;
            using (var chromeProcess = chromeProcessFactory.CreateLocal(9222, false))
            {
                currentProtocol = await chromeProcess.GetJsonAsync<ProtocolDefinition>("/json/protocol");
                await Task.Delay(1000); //Give the process some time to settle before we close it.
            }

            IProtocolGenerator generator = new ProtocolGenerator();
            generator.Generate(currentProtocol, TargetFolder);
        }
    }
}