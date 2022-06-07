using System.Collections;
using System.Collections.Generic;

namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentPeers : IEqualityComparer, IEqualityComparer<ITorrentPeers>
    {
        string Address { get; set; }
        string ClientName { get; set; }
        bool ClientIsChoked { get; set; }
        bool ClientIsInterested { get; set; }
        string FlagStr { get; set; }
        bool IsDownloadingFrom { get; set; }
        bool IsEncrypted { get; set; }
        bool IsUploadingTo { get; set; }
        bool IsUTP { get; set; }
        bool PeerIsChoked { get; set; }
        bool PeerIsInterested { get; set; }
        int Port { get; set; }
        double Progress { get; set; }
        int RateToClient { get; set; }
        int RateToPeer { get; set; }
    }
}