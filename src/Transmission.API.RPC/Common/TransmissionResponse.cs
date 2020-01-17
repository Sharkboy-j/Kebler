using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
