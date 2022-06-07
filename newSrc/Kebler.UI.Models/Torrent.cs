using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
    [DebuggerDisplay("{Name} | {RateDownload} {RateUpload}")]
    public class Torrent : PropertyChangedBase, ITorrent
    {
        private readonly uint _id;
        private long _addedDate;
        private int _bandwidthPriority;
        private string _comment;
        private int _corruptEver;
        private string _creator;
        private int _dateCreated;
        private long _desiredAvailable;
        private long _doneDate;
        private string _downloadDir;
        private long _downloadedEver;
        private long _downloadLimit;
        private bool _downloadLimited;
        private int _error;
        private string _errorString;
        private int _eta;
        private int _etaIdle;
        private IEnumerable<ITorrentFiles> _files;
        private IEnumerable<ITorrentFileStats> _fileStats;
        private string _hashString;
        private int _haveUnchecked;
        private long _haveValid;
        private bool _honorsSessionLimits;
        private bool _isFinished;
        private bool _isPrivate;
        private bool _isStalled;
        private long _leftUntilDone;
        private string _magnetLink;
        private int _manualAnnounceTime;
        private int _maxConnectedPeers;
        private double _metadataPercentComplete;
        private string _name;
        private int _peerLimit;
        private IEnumerable<ITorrentPeers> _peers;
        private int _peersConnected;
        private ITorrentPeersFrom _peersFrom;
        private int _peersSendingToUs;
        private double _percentDone;
        private string _pieces;
        private int _pieceCount;
        private int _pieceSize;
        private IEnumerable<int> _priorities;
        private int _queuePosition;
        private int _rateDownload;
        private int _rateUpload;
        private double _recheckProgress;
        private int _secondsDownloading;
        private int _secondsSeeding;
        private int _seedIdleLimit;
        private int _seedIdleMode;
        private double _seedRatioLimit;
        private int _seedRatioMode;
        private long _sizeWhenDone;
        private int _startDate;
        private int _status;
        private IEnumerable<ITorrentTrackers> _trackers;
        private IEnumerable<ITorrentTrackerStats> _trackerStats;
        private long _totalSize;
        private string _torrentFile;
        private long _uploadedEver;
        private long _uploadLimit;
        private bool _uploadLimited;
        private double _uploadRatio;
        private IEnumerable<bool> _wanted;
        private IEnumerable<string> _webSeeds;
        private int _webseedsSendingToUs;


        public Torrent(uint id, string name)
        {
            _id = id;
            Name = name;
        }

        public uint Id => _id;

        public long AddedDate
        {
            get => _addedDate;
            set => Set(ref _addedDate, value);
        }

        public int BandwidthPriority
        {
            get => _bandwidthPriority;
            set => Set(ref _bandwidthPriority, value);
        }

        public string Comment
        {
            get => _comment;
            set => Set(ref _comment, value);
        }

        public int CorruptEver
        {
            get => _corruptEver;
            set => Set(ref _corruptEver, value);
        }

        public string Creator
        {
            get => _creator;
            set => Set(ref _creator, value);
        }

        public int DateCreated
        {
            get => _dateCreated;
            set => Set(ref _dateCreated, value);
        }

        public long DesiredAvailable
        {
            get => _desiredAvailable;
            set => Set(ref _desiredAvailable, value);
        }

        public long DoneDate
        {
            get => _doneDate;
            set => Set(ref _doneDate, value);
        }

        public string DownloadDir
        {
            get => _downloadDir;
            set => Set(ref _downloadDir, value);
        }

        public long DownloadedEver
        {
            get => _downloadedEver;
            set => Set(ref _downloadedEver, value);

        }

        public long DownloadLimit
        {
            get => _downloadLimit;
            set => Set(ref _downloadLimit, value);

        }

        public bool DownloadLimited
        {
            get => _downloadLimited;
            set => Set(ref _downloadLimited, value);
        }

        public int Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string ErrorString
        {
            get => _errorString;
            set => Set(ref _errorString, value);
        }

        public int ETA
        {
            get => _eta;
            set => Set(ref _eta, value);
        }

        public int ETAIdle
        {
            get => _etaIdle;
            set => Set(ref _etaIdle, value);
        }

        public IEnumerable<ITorrentFiles> Files
        {
            get => _files;
            set => Set(ref _files, value);
        }

        public IEnumerable<ITorrentFileStats> FileStats
        {
            get => _fileStats;
            set => Set(ref _fileStats, value);
        }

        public string HashString
        {
            get => _hashString;
            set => Set(ref _hashString, value);
        }

        public int HaveUnchecked
        {
            get => _haveUnchecked;
            set => Set(ref _haveUnchecked, value);
        }

        public long HaveValid
        {
            get => _haveValid;
            set => Set(ref _haveValid, value);
        }

        public bool HonorsSessionLimits
        {
            get => _honorsSessionLimits;
            set => Set(ref _honorsSessionLimits, value);
        }

        public bool IsFinished
        {
            get => _isFinished;
            set => Set(ref _isFinished, value);
        }

        public bool IsPrivate
        {
            get => _isPrivate;
            set => Set(ref _isPrivate, value);
        }

        public bool IsStalled
        {
            get => _isStalled;
            set => Set(ref _isStalled, value);
        }

        public long LeftUntilDone
        {
            get => _leftUntilDone;
            set => Set(ref _leftUntilDone, value);
        }

        public string MagnetLink
        {
            get => _magnetLink;
            set => Set(ref _magnetLink, value);
        }

        public int ManualAnnounceTime
        {
            get => _manualAnnounceTime;
            set => Set(ref _manualAnnounceTime, value);
        }

        public int MaxConnectedPeers
        {
            get => _maxConnectedPeers;
            set => Set(ref _maxConnectedPeers, value);
        }

        public double MetadataPercentComplete
        {
            get => _metadataPercentComplete;
            set => Set(ref _metadataPercentComplete, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public int PeerLimit
        {
            get => _peerLimit;
            set => Set(ref _peerLimit, value);
        }

        public IEnumerable<ITorrentPeers> Peers
        {
            get => _peers;
            set => Set(ref _peers, value);
        }

        public int PeersConnected
        {
            get => _peersConnected;
            set => Set(ref _peersConnected, value);
        }

        public ITorrentPeersFrom PeersFrom
        {
            get => _peersFrom;
            set => Set(ref _peersFrom, value);
        }

        public int PeersSendingToUs
        {
            get => _peersSendingToUs;
            set => Set(ref _peersSendingToUs, value);
        }

        public double PercentDone
        {
            get => _percentDone;
            set => Set(ref _percentDone, value);
        }

        public string Pieces
        {
            get => _pieces;
            set => Set(ref _pieces, value);
        }

        public int PieceCount
        {
            get => _pieceCount;
            set => Set(ref _pieceCount, value);
        }

        public int PieceSize
        {
            get => _pieceSize;
            set => Set(ref _pieceSize, value);
        }

        public IEnumerable<int> Priorities
        {
            get => _priorities;
            set => Set(ref _priorities, value);
        }

        public int QueuePosition
        {
            get => _queuePosition;
            set => Set(ref _queuePosition, value);
        }

        public int RateDownload
        {
            get => _rateDownload;
            set => Set(ref _rateDownload, value);
        }

        public int RateUpload
        {
            get => _rateUpload;
            set => Set(ref _rateUpload, value);
        }

        public double RecheckProgress
        {
            get => _recheckProgress;
            set => Set(ref _recheckProgress, value);
        }

        public int SecondsDownloading
        {
            get => _secondsDownloading;
            set => Set(ref _secondsDownloading, value);
        }

        public int SecondsSeeding
        {
            get => _secondsSeeding;
            set => Set(ref _secondsSeeding, value);
        }

        public int SeedIdleLimit
        {
            get => _seedIdleLimit;
            set => Set(ref _seedIdleLimit, value);
        }

        public int SeedIdleMode
        {
            get => _seedIdleMode;
            set => Set(ref _seedIdleMode, value);
        }

        public double SeedRatioLimit
        {
            get => _seedRatioLimit;
            set => Set(ref _seedRatioLimit, value);
        }

        public int SeedRatioMode
        {
            get => _seedRatioMode;
            set => Set(ref _seedRatioMode, value);
        }

        public long SizeWhenDone
        {
            get => _sizeWhenDone;
            set => Set(ref _sizeWhenDone, value);
        }

        public int StartDate
        {
            get => _startDate;
            set => Set(ref _startDate, value);
        }

        /// <summary>
        /// <para>0: 'stopped'</para> 
        /// <para>1: 'check pending'</para> 
        /// <para>2: 'checking'</para> 
        /// <para>3: 'download pending'</para> 
        /// <para>4: 'downloading'</para> 
        /// <para>5: 'seed pending'</para> 
        /// <para>6: 'seeding' </para> 
        /// </summary>
        public int Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public IEnumerable<ITorrentTrackers> Trackers
        {
            get => _trackers;
            set => Set(ref _trackers, value);
        }

        public IEnumerable<ITorrentTrackerStats> TrackerStats
        {
            get => _trackerStats;
            set => Set(ref _trackerStats, value);
        }

        public long TotalSize
        {
            get => _totalSize;
            set => Set(ref _totalSize, value);
        }

        public string TorrentFile
        {
            get => _torrentFile;
            set => Set(ref _torrentFile, value);
        }

        public long UploadedEver
        {
            get => _uploadedEver;
            set => Set(ref _uploadedEver, value);
        }

        public long UploadLimit
        {
            get => _uploadLimit;
            set => Set(ref _uploadLimit, value);
        }

        public bool UploadLimited
        {
            get => _uploadLimited;
            set => Set(ref _uploadLimited, value);
        }

        public double UploadRatio
        {
            get => _uploadRatio;
            set => Set(ref _uploadRatio, value);
        }

        public IEnumerable<bool> Wanted
        {
            get => _wanted;
            set => Set(ref _wanted, value);
        }

        public IEnumerable<string> WebSeeds
        {
            get => _webSeeds;
            set => Set(ref _webSeeds, value);
        }

        public int WebseedsSendingToUs
        {
            get => _webseedsSendingToUs;
            set => Set(ref _webseedsSendingToUs, value);
        }

        public void Notify(ITorrent inf)
        {
            RateUpload = inf.RateUpload;
            RateDownload = inf.RateDownload;
            Name = inf.Name;
            PercentDone = inf.PercentDone;
            Status = inf.Status;
            UploadedEver = inf.UploadedEver;
            TotalSize = inf.TotalSize;
        }
        
        //TODO: Add tests!!!!!
        public override bool Equals(object obj)
        {
            if (obj is ITorrent torrentInfo)
                return torrentInfo.Id == Id &&
                       torrentInfo.HashString == HashString;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }
    }
}
