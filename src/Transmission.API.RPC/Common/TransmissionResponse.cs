using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Net;
using static Transmission.API.RPC.Entity.Enums;
using System;

namespace Transmission.API.RPC.Common
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

        public WebException WebException;

        public HttpWebResponse HttpWebResponse;

        public Exception CustomException;
    }
}
