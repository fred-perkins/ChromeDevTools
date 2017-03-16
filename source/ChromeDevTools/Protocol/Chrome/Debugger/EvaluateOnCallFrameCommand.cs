using MasterDevs.ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MasterDevs.ChromeDevTools.Protocol.Chrome.Debugger
{
	/// <summary>
	/// Evaluates expression on a given call frame.
	/// </summary>
	[Command(ProtocolName.Debugger.EvaluateOnCallFrame)]
	[SupportedBy("Chrome")]
	public class EvaluateOnCallFrameCommand
	{
		/// <summary>
		/// Gets or sets Call frame identifier to evaluate on.
		/// </summary>
		public string CallFrameId { get; set; }
		/// <summary>
		/// Gets or sets Expression to evaluate.
		/// </summary>
		public string Expression { get; set; }
		/// <summary>
		/// Gets or sets String object group name to put result into (allows rapid releasing resulting object handles using <code>releaseObjectGroup</code>).
		/// </summary>
		public string ObjectGroup { get; set; }
		/// <summary>
		/// Gets or sets Specifies whether command line API should be available to the evaluated expression, defaults to false.
		/// </summary>
		public bool IncludeCommandLineAPI { get; set; }
		/// <summary>
		/// Gets or sets In silent mode exceptions thrown during evaluation are not reported and do not pause execution. Overrides <code>setPauseOnException</code> state.
		/// </summary>
		public bool Silent { get; set; }
		/// <summary>
		/// Gets or sets Whether the result is expected to be a JSON object that should be sent by value.
		/// </summary>
		public bool ReturnByValue { get; set; }
		/// <summary>
		/// Gets or sets Whether preview should be generated for the result.
		/// </summary>
		public bool GeneratePreview { get; set; }
		/// <summary>
		/// Gets or sets Whether to throw an exception if side effect cannot be ruled out during evaluation.
		/// </summary>
		public bool ThrowOnSideEffect { get; set; }
	}
}
