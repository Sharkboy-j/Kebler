namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentFileStats
    {
        double BytesCompleted { get; set; }
        int Priority { get; set; }
        bool Wanted { get; set; }
    }
}