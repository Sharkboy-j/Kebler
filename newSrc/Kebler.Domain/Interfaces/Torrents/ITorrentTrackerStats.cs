using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentTrackerStats : IEqualityComparer, IEqualityComparer<ITorrentTrackerStats>
    {
        string announce { get; set; }
        int AnnounceState { get; set; }
        int DownloadCount { get; set; }
        bool HasAnnounced { get; set; }
        bool HasScraped { get; set; }
        string Host { get; set; }
        bool IsBackup { get; set; }
        int LastAnnouncePeerCount { get; set; }
        uint ID { get; set; }
        string LastAnnounceResult { get; set; }
        bool LastAnnounceSucceeded { get; set; }
        int LastAnnounceStartTime { get; set; }
        string LastScrapeResult { get; set; }
        bool LastAnnounceTimedOut { get; set; }
        int LastAnnounceTime { get; set; }
        bool LastScrapeSucceeded { get; set; }
        int LastScrapeStartTime { get; set; }
        bool LastScrapeTimedOut { get; set; }
        int LastScrapeTime { get; set; }
        string Scrape { get; set; }
        int Tier { get; set; }
        int LeecherCount { get; set; }
        long NextAnnounceTime { get; set; }
        long NextScrapeTime { get; set; }
        int ScrapeState { get; set; }
        int SeederCount { get; set; }
    }
}