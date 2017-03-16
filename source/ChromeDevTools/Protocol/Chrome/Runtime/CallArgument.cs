using MasterDevs.ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime
{
	/// <summary>
	/// Represents function call argument. Either remote object id <code>objectId</code>, primitive <code>value</code>, unserializable primitive value or neither of (for undefined) them should be specified.
	/// </summary>
	[SupportedBy("Chrome")]
	public class CallArgument
	{
		/// <summary>
		/// Gets or sets Primitive value.
		/// </summary>
		public object Value { get; set; }
		/// <summary>
		/// Gets or sets Primitive value which can not be JSON-stringified.
		/// </summary>
		public UnserializableValue UnserializableValue { get; set; }
		/// <summary>
		/// Gets or sets Remote object handle.
		/// </summary>
		public string ObjectId { get; set; }
	}
}
