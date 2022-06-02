using System;
using Kebler.Transmission.Models.Entities;

namespace Kebler.Transmission.Models
{
    public sealed class AddTorrentResponse : EntityBase<TorrentAddResult>
    {
        public Exception CustomException { get; set; }

        public AddTorrentResponse(Enums.ReponseResult val)
        {
            Result = val;
        }
    }
}