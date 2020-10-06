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
}