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

    public sealed class TransmissionInfoResponse<T> : TransmissionResponse
    {
        public TransmissionInfoResponse(T s)
        {
            Value = s;
        }
        public T Value;
    }
}
