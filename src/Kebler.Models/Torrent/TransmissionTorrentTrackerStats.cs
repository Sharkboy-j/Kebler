using System.ComponentModel;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentTrackerStats : PropertyChangedBase
    {
        private string _announce;
        private int _announceState;
        private int _downloadCount;

        [JsonProperty("announce")]
        public string announce
        {
            get => _announce;
            set => Set(ref _announce, value);
        }

        [JsonProperty("announceState")]
        public int AnnounceState
        {
            get => _announceState;
            set => Set(ref _announceState, value);
        }

        [JsonProperty("downloadCount")]
        public int DownloadCount
        {
            get => _downloadCount;
            set => Set(ref _downloadCount, value);
        }

        [JsonProperty("hasAnnounced")]
        public bool HasAnnounced { get; set; }

        [JsonProperty("hasScraped")]
        public bool HasScraped { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("isBackup")]
        public bool IsBackup { get; set; }

        [JsonProperty("lastAnnouncePeerCount")]
        public int LastAnnouncePeerCount { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("lastAnnounceResult")]
        public string LastAnnounceResult { get; set; }

        [JsonProperty("lastAnnounceSucceeded")]
        public bool LastAnnounceSucceeded { get; set; }

        [JsonProperty("lastAnnounceStartTime")]
        public int LastAnnounceStartTime { get; set; }

        [JsonProperty("lastScrapeResult")]
        public string LastScrapeResult { get; set; }

        [JsonProperty("lastAnnounceTimedOut")]
        public bool LastAnnounceTimedOut { get; set; }

        [JsonProperty("lastAnnounceTime")]
        public int LastAnnounceTime { get; set; }

        [JsonProperty("lastScrapeSucceeded")]
        public bool LastScrapeSucceeded { get; set; }

        [JsonProperty("lastScrapeStartTime")]
        public int LastScrapeStartTime { get; set; }

        [JsonProperty("lastScrapeTimedOut")]
        public bool LastScrapeTimedOut { get; set; }

        [JsonProperty("lastScrapeTime")]
        public int LastScrapeTime { get; set; }

        [JsonProperty("scrape")]
        public string Scrape { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }

        [JsonProperty("leecherCount")]
        public int LeecherCount { get; set; }

        [JsonProperty("nextAnnounceTime")]
        public int NextAnnounceTime { get; set; }

        [JsonProperty("nextScrapeTime")]
        public int NextScrapeTime { get; set; }

        [JsonProperty("scrapeState")]
        public int ScrapeState { get; set; }

        [JsonProperty("seederCount")]
        public int SeederCount { get; set; }


        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }
    }
}