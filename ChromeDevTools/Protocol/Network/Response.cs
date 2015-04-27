using ChromeDevTools;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChromeDevTools.Protocol.Network
{
	/// <summary>
	/// HTTP response data.
	/// </summary>
	public class Response
	{
		/// <summary>
		/// Gets or sets Response URL. This URL can be different from CachedResource.url in case of redirect.
		/// </summary>
		public string Url { get; set; }
		/// <summary>
		/// Gets or sets HTTP response status code.
		/// </summary>
		public double Status { get; set; }
		/// <summary>
		/// Gets or sets HTTP response status text.
		/// </summary>
		public string StatusText { get; set; }
		/// <summary>
		/// Gets or sets HTTP response headers.
		/// </summary>
		public Dictionary<string, string> Headers { get; set; }
		/// <summary>
		/// Gets or sets HTTP response headers text.
		/// </summary>
		public string HeadersText { get; set; }
		/// <summary>
		/// Gets or sets Resource mimeType as determined by the browser.
		/// </summary>
		public string MimeType { get; set; }
		/// <summary>
		/// Gets or sets Refined HTTP request headers that were actually transmitted over the network.
		/// </summary>
		public Dictionary<string, string> RequestHeaders { get; set; }
		/// <summary>
		/// Gets or sets HTTP request headers text.
		/// </summary>
		public string RequestHeadersText { get; set; }
		/// <summary>
		/// Gets or sets Specifies whether physical connection was actually reused for this request.
		/// </summary>
		public bool ConnectionReused { get; set; }
		/// <summary>
		/// Gets or sets Physical connection id that was actually used for this request.
		/// </summary>
		public double ConnectionId { get; set; }
		/// <summary>
		/// Gets or sets Remote IP address.
		/// </summary>
		public string RemoteIPAddress { get; set; }
		/// <summary>
		/// Gets or sets Remote port.
		/// </summary>
		public long RemotePort { get; set; }
		/// <summary>
		/// Gets or sets Specifies that the request was served from the disk cache.
		/// </summary>
		public bool FromDiskCache { get; set; }
		/// <summary>
		/// Gets or sets Specifies that the request was served from the ServiceWorker.
		/// </summary>
		public bool FromServiceWorker { get; set; }
		/// <summary>
		/// Gets or sets Total number of bytes received for this request so far.
		/// </summary>
		public double EncodedDataLength { get; set; }
		/// <summary>
		/// Gets or sets Timing information for the given request.
		/// </summary>
		public ResourceTiming Timing { get; set; }
		/// <summary>
		/// Gets or sets Protocol used to fetch this resquest.
		/// </summary>
		public string Protocol { get; set; }
	}
}