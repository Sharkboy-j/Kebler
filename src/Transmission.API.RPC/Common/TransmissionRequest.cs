using System.Collections.Generic;
using Newtonsoft.Json;

namespace Transmission.API.RPC.Common
{
	/// <summary>
	/// Transmission request 
	/// </summary>
	public class TransmissionRequest : CommunicateBase
	{
		/// <summary>
		/// Name of the method to invoke
		/// </summary>
		[JsonProperty("method")]
		public string Method;

        public TransmissionRequest(string method)
        {
            Method = method;
        }

		public TransmissionRequest(string method, ArgumentsBase arguments)
		{
			Method = method;
			Arguments = arguments.Data;
		}

        public TransmissionRequest(string method, Dictionary<string, object> arguments)
        {
            Method = method;
            Arguments = arguments;
        }
	}
}
