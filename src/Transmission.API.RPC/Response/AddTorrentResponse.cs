using Transmission.API.RPC.Common;
using Transmission.API.RPC.Entity;

namespace Transmission.API.RPC.Response
{
    public sealed class AddTorrentResponse : EntityBase<TorrentAddResult>
    {
        public AddTorrentResponse(Enums.ReponseResult  result)
        {
            Result = result;
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
