using Kebler.Core.Models.Entities;
using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.Core.Models
{
    public sealed class AddTorrentResponse : EntityBase<TorrentAddResult>
    {
        public AddTorrentResponse(Enums.ReponseResult val)
        {
            Result = val;
        }
    }
}