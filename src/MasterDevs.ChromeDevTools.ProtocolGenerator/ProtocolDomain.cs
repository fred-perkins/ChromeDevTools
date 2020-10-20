using System;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public class ProtocolDomain : ProtocolItem
    {
        public ProtocolDomain()
        {
            this.Types = new Collection<ProtocolType>();
            this.Events = new Collection<ProtocolEvent>();
            this.Commands = new Collection<ProtocolCommand>();
        }

        [JsonProperty(PropertyName = "domain")]
        public override string Name
        {
            get;
            set;
        }

        public Collection<ProtocolType> Types
        {
            get;
            set;
        }

        public Collection<ProtocolCommand> Commands
        {
            get;
            set;
        }

        public Collection<ProtocolEvent> Events
        {
            get;
            set;
        }

        public string Availability
        {
            get;
            set;
        }

        public string FeatureGuard
        {
            get;
            set;
        }

        [JsonProperty("experimental")]
        public bool IsExperimental { get; set; }

        [JsonProperty("deprecated")]
        public bool IsDeprecated { get; set; }


        public string[] Dependencies { get; set; }

        public ProtocolCommand GetCommand(string name)
        {
            return this.Commands.SingleOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public ProtocolType GetType(string name)
        {
            return this.Types.SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
