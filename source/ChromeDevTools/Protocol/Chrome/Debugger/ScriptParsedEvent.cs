using MasterDevs.ChromeDevTools;

namespace MasterDevs.ChromeDevTools.Protocol.Chrome.Debugger
{
	/// <summary>
	/// Fired when virtual machine parses script. This event is also fired for all known and uncollected scripts upon enabling debugger.
	/// </summary>
	[Event(ProtocolName.Debugger.ScriptParsed)]
	[SupportedBy("Chrome")]
	public class ScriptParsedEvent
	{
		/// <summary>
		/// Gets or sets Identifier of the script parsed.
		/// </summary>
		public string ScriptId { get; set; }
		/// <summary>
		/// Gets or sets URL or name of the script parsed (if any).
		/// </summary>
		public string Url { get; set; }
		/// <summary>
		/// Gets or sets Line offset of the script within the resource with given URL (for script tags).
		/// </summary>
		public long StartLine { get; set; }
		/// <summary>
		/// Gets or sets Column offset of the script within the resource with given URL.
		/// </summary>
		public long StartColumn { get; set; }
		/// <summary>
		/// Gets or sets Last line of the script.
		/// </summary>
		public long EndLine { get; set; }
		/// <summary>
		/// Gets or sets Length of the last line of the script.
		/// </summary>
		public long EndColumn { get; set; }
		/// <summary>
		/// Gets or sets Specifies script creation context.
		/// </summary>
		public long ExecutionContextId { get; set; }
		/// <summary>
		/// Gets or sets Content hash of the script.
		/// </summary>
		public string Hash { get; set; }
		/// <summary>
		/// Gets or sets Embedder-specific auxiliary data.
		/// </summary>
		public object ExecutionContextAuxData { get; set; }
		/// <summary>
		/// Gets or sets True, if this script is generated as a result of the live edit operation.
		/// </summary>
		public bool IsLiveEdit { get; set; }
		/// <summary>
		/// Gets or sets URL of source map associated with script (if any).
		/// </summary>
		public string SourceMapURL { get; set; }
		/// <summary>
		/// Gets or sets True, if this script has sourceURL.
		/// </summary>
		public bool HasSourceURL { get; set; }
		/// <summary>
		/// Gets or sets True, if this script is ES6 module.
		/// </summary>
		public bool IsModule { get; set; }
		/// <summary>
		/// Gets or sets This script length.
		/// </summary>
		public long Length { get; set; }
	}
}
