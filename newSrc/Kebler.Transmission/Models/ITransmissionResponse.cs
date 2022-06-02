using System;
using System.Net;

namespace Kebler.Transmission.Models
{
    public interface ITransmissionResponse
    {
        public string Result { get; set; }

        public WebException WebException { get; set; }

        public HttpWebResponse HttpWebResponse { get; set; }

        public Exception CustomException { get; set; }

        public bool Success { get; }

        public string Method { get; set; }
    }
}