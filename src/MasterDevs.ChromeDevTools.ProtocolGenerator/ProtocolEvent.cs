using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public class ProtocolEvent : ProtocolItem
    {
        public ProtocolEvent()
        {
            this.Parameters = new Collection<ProtocolProperty>();
            this.Handlers = new Collection<string>();
        }

        public Collection<ProtocolProperty> Parameters
        {
            get;
            set;
        }

        public Collection<string> Handlers
        {
            get;
            set;
        }

        public bool Deprecated
        {
            get;
            set;
        }

        [JsonProperty("experimental")]
        public bool IsExperimental { get; set; }
    }
}
