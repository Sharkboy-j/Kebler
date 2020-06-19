using Newtonsoft.Json;
using System.Net;
using System;

namespace Kebler.Models.Torrent.Common
{
	/// <summary>
	/// Transmission response 
	/// </summary>
	public class TransmissionResponse : CommunicateBase, ITransmissionReponse
	{
		/// <summary>
		/// Contains "success" on success, or an error string on failure.
		/// </summary>
		[JsonProperty("result")]
		public string Result { get; set; }

        [JsonIgnore]
        public WebException WebException { get; set; }

        [JsonIgnore]
        public HttpWebResponse HttpWebResponse { get; set; }

        [JsonIgnore]
        public Exception CustomException { get; set; }

        [JsonIgnore]
		public bool Success  => this.Result == "success";
        
        [JsonIgnore]
        public string Method { get; set; }
    }
}
