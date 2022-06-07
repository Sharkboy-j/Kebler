namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentTrackers
    {
        string announce { get; set; }
        uint ID { get; set; }
        string Scrape { get; set; }
        int Tier { get; set; }

        bool Equals(object obj);
        int GetHashCode();
    }
}