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
}
