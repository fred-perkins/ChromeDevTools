using System.Collections.ObjectModel;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public class ProtocolDefinition
    {
        public ProtocolVersion Version { get; set; }

        public Collection<ProtocolDomain> Domains { get; set; } = new Collection<ProtocolDomain>();
    }
}
