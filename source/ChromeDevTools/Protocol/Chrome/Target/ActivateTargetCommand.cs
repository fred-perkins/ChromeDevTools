using MasterDevs.ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MasterDevs.ChromeDevTools.Protocol.Chrome.Target
{
	/// <summary>
	/// Activates (focuses) the target.
	/// </summary>
	[Command(ProtocolName.Target.ActivateTarget)]
	[SupportedBy("Chrome")]
	public class ActivateTargetCommand
	{
		/// <summary>
		/// Gets or sets TargetId
		/// </summary>
		public string TargetId { get; set; }
	}
}
