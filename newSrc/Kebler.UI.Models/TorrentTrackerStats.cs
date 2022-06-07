using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;
using Newtonsoft.Json;

namespace Kebler.UI.Models
{
    public class TorrentTrackerStats : PropertyChangedBase, ITorrentTrackerStats
    {
        private string _announce;
        private int _announceState;
        private int _downloadCount;
        private bool _hasAnnounced;
        private bool _hasScraped;
        private string _host;
        private bool _isBackup;
        private int _lastAnnouncePeerCount;
        private uint _id;
        private string _lastAnnounceResult;
        private bool _lastAnnounceSucceeded;
        private int _lastAnnounceStartTime;
        private string _lastScrapeResult;
        private bool _lastAnnounceTimedOut;
        private int _lastAnnounceTime;
        private bool _lastScrapeSucceeded;
        private int _lastScrapeStartTime;
        private bool _lastScrapeTimedOut;
        private int _lastScrapeTime;
        private string _scrape;
        private int _tier;
        private int _leecherCount;
        private long _nextAnnounceTime;
        private long _nextScrapeTime;
        private int _scrapeState;
        private int _seederCount;

        public string announce
        {
            get => _announce;
            set => Set(ref _announce, value);
        }

        public int AnnounceState
        {
            get => _announceState;
            set => Set(ref _announceState, value);
        }

        public int DownloadCount
        {
            get => _downloadCount;
            set => Set(ref _downloadCount, value);
        }

        public bool HasAnnounced
        {
            get => _hasAnnounced;
            set => Set(ref _hasAnnounced, value);
        }

        public bool HasScraped
        {
            get => _hasScraped;
            set => Set(ref _hasScraped, value);
        }

        public string Host
        {
            get => _host;
            set => Set(ref _host, value);
        }

        public bool IsBackup
        {
            get => _isBackup;
            set => Set(ref _isBackup, value);
        }

        public int LastAnnouncePeerCount
        {
            get => _lastAnnouncePeerCount;
            set => Set(ref _lastAnnouncePeerCount, value);
        }

        public uint ID
        {
            get => _id;
            set => Set(ref _id, value);
        }

        public string LastAnnounceResult
        {
            get => _lastAnnounceResult;
            set => Set(ref _lastAnnounceResult, value);
        }

        public bool LastAnnounceSucceeded
        {
            get => _lastAnnounceSucceeded;
            set => Set(ref _lastAnnounceSucceeded, value);
        }

        public int LastAnnounceStartTime
        {
            get => _lastAnnounceStartTime;
            set => Set(ref _lastAnnounceStartTime, value);
        }

        public string LastScrapeResult
        {
            get => _lastScrapeResult;
            set => Set(ref _lastScrapeResult, value);
        }

        public bool LastAnnounceTimedOut
        {
            get => _lastAnnounceTimedOut;
            set => Set(ref _lastAnnounceTimedOut, value);
        }

        public int LastAnnounceTime
        {
            get => _lastAnnounceTime;
            set => Set(ref _lastAnnounceTime, value);
        }

        public bool LastScrapeSucceeded
        {
            get => _lastScrapeSucceeded;
            set => Set(ref _lastScrapeSucceeded, value);
        }

        public int LastScrapeStartTime
        {
            get => _lastScrapeStartTime;
            set => Set(ref _lastScrapeStartTime, value);
        }

        public bool LastScrapeTimedOut
        {
            get => _lastScrapeTimedOut;
            set => Set(ref _lastScrapeTimedOut, value);
        }

        public int LastScrapeTime
        {
            get => _lastScrapeTime;
            set => Set(ref _lastScrapeTime, value);
        }

        public string Scrape
        {
            get => _scrape;
            set => Set(ref _scrape, value);
        }

        public int Tier
        {
            get => _tier;
            set => Set(ref _tier, value);
        }

        public int LeecherCount
        {
            get => _leecherCount;
            set => Set(ref _leecherCount, value);
        }

        public long NextAnnounceTime
        {
            get => _nextAnnounceTime;
            set => Set(ref _nextAnnounceTime, value);
        }

        public long NextScrapeTime
        {
            get => _nextScrapeTime;
            set => Set(ref _nextScrapeTime, value);
        }

        public int ScrapeState
        {
            get => _scrapeState;
            set => Set(ref _scrapeState, value);
        }

        public int SeederCount
        {
            get => _seederCount;
            set => Set(ref _seederCount, value);
        }

        public new bool Equals(object x, object y)
        {
            if (x is TorrentTrackerStats torrentTrackerStats && y is TorrentTrackerStats trackerStats)
                return Equals(torrentTrackerStats, trackerStats);
            return false;
        }

     

        public int GetHashCode(object obj)
        {
            if (obj is TorrentTrackerStats st)
                return st.GetHashCode();

            return obj.GetHashCode();
        }


        public int GetHashCode([DisallowNull] ITorrentTrackerStats obj)
        {
            return obj.GetHashCode();
        }


        public bool Equals([AllowNull] ITorrentTrackerStats x, [AllowNull] ITorrentTrackerStats y)
        {
            return x != null && y != null && x.ID == y.ID;
        }


        public override bool Equals(object obj)
        {
            if (obj is TorrentTrackerStats st) return Equals(this, st);
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
