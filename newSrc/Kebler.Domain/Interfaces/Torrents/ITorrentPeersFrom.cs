namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentPeersFrom
    {
        int FromDHT { get; set; }
        int FromIncoming { get; set; }
        int FromLPD { get; set; }
        int FromLTEP { get; set; }
        int FromPEX { get; set; }
        int FromTracker { get; set; }
    }
}