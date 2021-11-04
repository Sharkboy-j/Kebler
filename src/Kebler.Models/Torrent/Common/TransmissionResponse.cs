using System;
using System.Net;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Common
{
    /// <summary>
    ///     Transmission response
    /// </summary>
    public class TransmissionResponse : CommunicateBase, ITransmissionReponse
    {
        /// <summary>
        ///     Contains "success" on success, or an error string on failure.
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
        public bool Success => Result == "success";

        [JsonIgnore]
        public string Method { get; set; }
    }

    public class TransmissionResponse<T>
    {
        public TransmissionResponse(TransmissionResponse t)
        {
            Response = t;
            Value = t.Deserialize<T>();
        }

        public TransmissionResponse Response { get; set; }

        public T Value { get; set; }
    }
}