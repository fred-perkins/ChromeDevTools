using System.Linq;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public static class ProtocolExtensions
    {
        public static ProtocolDomain GetDomain(this ProtocolDefinition protocol, string name)
        {
            return protocol.Domains.SingleOrDefault(d => string.Equals(d.Name, name, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
