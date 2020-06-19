using Newtonsoft.Json;
using System.Net;
using System;

namespace Kebler.Models.Torrent.Common
{
	/// <summary>
	/// Transmission response 
	/// </summary>
	public class TransmissionResponse : CommunicateBase
	{
		/// <summary>
		/// Contains "success" on success, or an error string on failure.
		/// </summary>
		[JsonProperty("result")]
		public string Result;

        [JsonIgnore]
        public WebException WebException;

        [JsonIgnore]
        public HttpWebResponse HttpWebResponse;

        [JsonIgnore]
        public Exception CustomException;

		[JsonIgnore]
		public bool Success { get; set; } = false;
    }
}
