using ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChromeDevTools.Protocol.Input
{
	/// <summary>
	/// Dispatches a key event to the page.
	/// </summary>
	[Command(ProtocolName.Input.DispatchKeyEvent)]
	public class DispatchKeyEventCommand
	{
		/// <summary>
		/// Gets or sets Type of the key event.
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// Gets or sets Bit field representing pressed modifier keys. Alt=1, Ctrl=2, Meta/Command=4, Shift=8 (default: 0).
		/// </summary>
		public long Modifiers { get; set; }
		/// <summary>
		/// Gets or sets Time at which the event occurred. Measured in UTC time in seconds since January 1, 1970 (default: current time).
		/// </summary>
		public double Timestamp { get; set; }
		/// <summary>
		/// Gets or sets Text as generated by processing a virtual key code with a keyboard layout. Not needed for for <code>keyUp</code> and <code>rawKeyDown</code> events (default: "")
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets Text that would have been generated by the keyboard if no modifiers were pressed (except for shift). Useful for shortcut (accelerator) key handling (default: "").
		/// </summary>
		public string UnmodifiedText { get; set; }
		/// <summary>
		/// Gets or sets Unique key identifier (e.g., 'U+0041') (default: "").
		/// </summary>
		public string KeyIdentifier { get; set; }
		/// <summary>
		/// Gets or sets Unique DOM defined string value for each physical key (e.g., 'KeyA') (default: "").
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// Gets or sets Windows virtual key code (default: 0).
		/// </summary>
		public long WindowsVirtualKeyCode { get; set; }
		/// <summary>
		/// Gets or sets Native virtual key code (default: 0).
		/// </summary>
		public long NativeVirtualKeyCode { get; set; }
		/// <summary>
		/// Gets or sets Whether the event was generated from auto repeat (default: false).
		/// </summary>
		public bool AutoRepeat { get; set; }
		/// <summary>
		/// Gets or sets Whether the event was generated from the keypad (default: false).
		/// </summary>
		public bool IsKeypad { get; set; }
		/// <summary>
		/// Gets or sets Whether the event was a system key event (default: false).
		/// </summary>
		public bool IsSystemKey { get; set; }
	}
}