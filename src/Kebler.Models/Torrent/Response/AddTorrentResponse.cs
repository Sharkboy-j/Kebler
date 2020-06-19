using System;
using System.Net;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;

namespace Kebler.Models.Torrent.Response
{
    public sealed class AddTorrentResponse : EntityBase<TorrentAddResult>
    {
        public AddTorrentResponse(Enums.ReponseResult val)
        {
            Result = val;
        }
    }

    public sealed class TransmissionInfoResponse<T> : ITransmissionReponse
    {
        public T Value;
        public string Result { get; set; }
        public WebException WebException { get; set; }
        public HttpWebResponse HttpWebResponse { get; set; }
        public Exception CustomException { get; set; }
        public bool Success { get; set; }
        public string Method { get; set; }
    }
}
