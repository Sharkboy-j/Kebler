using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
  
    public class TorrentClientSettings : PropertyChangedBase, ITorrentClientSettings
    {
        private long _alternativeSpeedDown;
        private bool? _alternativeSpeedEnabled;
        private int? _alternativeSpeedTimeBegin;
        private bool? _alternativeSpeedTimeEnabled;
        private int? _alternativeSpeedTimeEnd;
        private int? _alternativeSpeedTimeDay;
        private long _alternativeSpeedUp;
        private string _blocklistUrl;
        private bool? _blocklistEnabled;
        private int? _cacheSizeMb;
        private string _downloadDirectory;
        private int? _downloadQueueSize;
        private bool? _downloadQueueEnabled;
        private bool? _dhtEnabled;
        private string _encryption;
        private int? _idleSeedingLimit;
        private bool? _idleSeedingLimitEnabled;
        private string _incompleteDirectory;
        private bool? _incompleteDirectoryEnabled;
        private bool? _lpdEnabled;
        private int? _peerLimitGlobal;
        private int? _peerLimitPerTorrent;
        private bool? _pexEnabled;
        private int? _peerPort;
        private bool? _peerPortRandomOnStart;
        private bool? _portForwardingEnabled;
        private bool? _queueStalledEnabled;
        private int? _queueStalledMinutes;
        private bool? _renamePartialFiles;
        private string _scriptTorrentDoneFilename;
        private bool? _scriptTorrentDoneEnabled;
        private double? _seedRatioLimit;
        private bool? _seedRatioLimited;
        private int? _seedQueueSize;
        private bool? _seedQueueEnabled;
        private int? _speedLimitDown;
        private bool? _speedLimitDownEnabled;
        private int? _speedLimitUp;
        private bool? _speedLimitUpEnabled;
        private bool? _startAddedTorrents;
        private bool? _trashOriginalTorrentFiles;
        private bool? _utpEnabled;

        public long AlternativeSpeedDown
        {
            get => _alternativeSpeedDown;
            set => Set(ref _alternativeSpeedDown, value);
        }

        public bool? AlternativeSpeedEnabled
        {
            get => _alternativeSpeedEnabled;
            set => Set(ref _alternativeSpeedEnabled, value);
        }

        public int? AlternativeSpeedTimeBegin
        {
            get => _alternativeSpeedTimeBegin;
            set => Set(ref _alternativeSpeedTimeBegin, value);
        }

        public bool? AlternativeSpeedTimeEnabled
        {
            get => _alternativeSpeedTimeEnabled;
            set => Set(ref _alternativeSpeedTimeEnabled, value);
        }

        public int? AlternativeSpeedTimeEnd
        {
            get => _alternativeSpeedTimeEnd;
            set => Set(ref _alternativeSpeedTimeEnd, value);
        }

        public int? AlternativeSpeedTimeDay
        {
            get => _alternativeSpeedTimeDay;
            set => Set(ref _alternativeSpeedTimeDay, value);
        }

        public long AlternativeSpeedUp
        {
            get => _alternativeSpeedUp;
            set => Set(ref _alternativeSpeedUp, value);
        }

        public string BlocklistURL
        {
            get => _blocklistUrl;
            set => Set(ref _blocklistUrl, value);
        }

        public bool? BlocklistEnabled
        {
            get => _blocklistEnabled;
            set => Set(ref _blocklistEnabled, value);
        }

        public int? CacheSizeMB
        {
            get => _cacheSizeMb;
            set => Set(ref _cacheSizeMb, value);
        }

        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set => Set(ref _downloadDirectory, value);
        }

        public int? DownloadQueueSize
        {
            get => _downloadQueueSize;
            set => Set(ref _downloadQueueSize, value);
        }

        public bool? DownloadQueueEnabled
        {
            get => _downloadQueueEnabled;
            set => Set(ref _downloadQueueEnabled, value);
        }

        public bool? DHTEnabled
        {
            get => _dhtEnabled;
            set => Set(ref _dhtEnabled, value);
        }

        public string Encryption
        {
            get => _encryption;
            set => Set(ref _encryption, value);
        }

        public int? IdleSeedingLimit
        {
            get => _idleSeedingLimit;
            set => Set(ref _idleSeedingLimit, value);
        }

        public bool? IdleSeedingLimitEnabled
        {
            get => _idleSeedingLimitEnabled;
            set => Set(ref _idleSeedingLimitEnabled, value);
        }

        public string IncompleteDirectory
        {
            get => _incompleteDirectory;
            set => Set(ref _incompleteDirectory, value);
        }

        public bool? IncompleteDirectoryEnabled
        {
            get => _incompleteDirectoryEnabled;
            set => Set(ref _incompleteDirectoryEnabled, value);
        }

        public bool? LPDEnabled
        {
            get => _lpdEnabled;
            set => Set(ref _lpdEnabled, value);
        }

        public int? PeerLimitGlobal
        {
            get => _peerLimitGlobal;
            set => Set(ref _peerLimitGlobal, value);
        }

        public int? PeerLimitPerTorrent
        {
            get => _peerLimitPerTorrent;
            set => Set(ref _peerLimitPerTorrent, value);
        }

        public bool? PexEnabled
        {
            get => _pexEnabled;
            set => Set(ref _pexEnabled, value);
        }

        public int? PeerPort
        {
            get => _peerPort;
            set => Set(ref _peerPort, value);
        }

        public bool? PeerPortRandomOnStart
        {
            get => _peerPortRandomOnStart;
            set => Set(ref _peerPortRandomOnStart, value);
        }

        public bool? PortForwardingEnabled
        {
            get => _portForwardingEnabled;
            set => Set(ref _portForwardingEnabled, value);
        }

        public bool? QueueStalledEnabled
        {
            get => _queueStalledEnabled;
            set => Set(ref _queueStalledEnabled, value);
        }

        public int? QueueStalledMinutes
        {
            get => _queueStalledMinutes;
            set => Set(ref _queueStalledMinutes, value);
        }

        public bool? RenamePartialFiles
        {
            get => _renamePartialFiles;
            set => Set(ref _renamePartialFiles, value);
        }

        public string ScriptTorrentDoneFilename
        {
            get => _scriptTorrentDoneFilename;
            set => Set(ref _scriptTorrentDoneFilename, value);
        }

        public bool? ScriptTorrentDoneEnabled
        {
            get => _scriptTorrentDoneEnabled;
            set => Set(ref _scriptTorrentDoneEnabled, value);
        }

        public double? SeedRatioLimit
        {
            get => _seedRatioLimit;
            set => Set(ref _seedRatioLimit, value);
        }

        public bool? SeedRatioLimited
        {
            get => _seedRatioLimited;
            set => Set(ref _seedRatioLimited, value);
        }

        public int? SeedQueueSize
        {
            get => _seedQueueSize;
            set => Set(ref _seedQueueSize, value);
        }

        public bool? SeedQueueEnabled
        {
            get => _seedQueueEnabled;
            set => Set(ref _seedQueueEnabled, value);
        }

        public int? SpeedLimitDown
        {
            get => _speedLimitDown;
            set => Set(ref _speedLimitDown, value);
        }

        public bool? SpeedLimitDownEnabled
        {
            get => _speedLimitDownEnabled;
            set => Set(ref _speedLimitDownEnabled, value);
        }

        public int? SpeedLimitUp
        {
            get => _speedLimitUp;
            set => Set(ref _speedLimitUp, value);
        }

        public bool? SpeedLimitUpEnabled
        {
            get => _speedLimitUpEnabled;
            set => Set(ref _speedLimitUpEnabled, value);
        }

        public bool? StartAddedTorrents
        {
            get => _startAddedTorrents;
            set => Set(ref _startAddedTorrents, value);
        }

        public bool? TrashOriginalTorrentFiles
        {
            get => _trashOriginalTorrentFiles;
            set => Set(ref _trashOriginalTorrentFiles, value);
        }

        public bool? UtpEnabled
        {
            get => _utpEnabled;
            set => Set(ref _utpEnabled, value);
        }
    }
}