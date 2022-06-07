using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;
using Newtonsoft.Json;

namespace Kebler.UI.Models
{
    public class Statistic : PropertyChangedBase, IStatistic
    {
        private long _activeTorrentCount;
        private long _downloadSpeed;
        private long _uploadSpeed;
        private long _pausedTorrentCount;
        private long _torrentCount;
        private long _uploadedBytes;
        private long _downloadedBytes;
        private long _filesAdded;
        private long _uptime;
        private long _alternativeSpeedDown;
        private bool _alternativeSpeedEnabled;
        private long _alternativeSpeedTimeBegin;
        private bool _alternativeSpeedTimeEnabled;
        private long _alternativeSpeedTimeEnd;
        private long _alternativeSpeedTimeDay;
        private long _alternativeSpeedUp;
        private string _blocklistUrl;
        private bool _blocklistEnabled;
        private long _cacheSizeMb;
        private string _downloadDirectory;
        private long _downloadQueueSize;
        private bool _downloadQueueEnabled;
        private bool _dhtEnabled;
        private string _encryption;
        private long _idleSeedingLimit;
        private bool _idleSeedingLimitEnabled;
        private string _incompleteDirectory;
        private bool _incompleteDirectoryEnabled;
        private bool _lpdEnabled;
        private long _peerLimitGlobal;
        private long _peerLimitPerTorrent;
        private bool _pexEnabled;
        private long _peerPort;
        private bool _peerPortRandomOnStart;
        private bool _portForwardingEnabled;
        private bool _queueStalledEnabled;
        private long _queueStalledMinutes;
        private bool _renamePartialFiles;
        private string _scriptTorrentDoneFilename;
        private bool _scriptTorrentDoneEnabled;
        private double _seedRatioLimit;
        private bool _seedRatioLimited;
        private long _seedQueueSize;
        private bool _seedQueueEnabled;
        private long _speedLimitDown;
        private bool _speedLimitDownEnabled;
        private long _speedLimitUp;
        private bool _speedLimitUpEnabled;
        private bool _startAddedTorrents;
        private bool _trashOriginalTorrentFiles;
        private bool _utpEnabled;
        private long _blocklistSize;
        private string _configDirectory;
        private long _rpcVersion;
        private long _rpcVersionMinimum;
        private string _version;

        public long ActiveTorrentCount
        {
            get => _activeTorrentCount;
            set => Set(ref _activeTorrentCount, value);
        }

        public long DownloadSpeed
        {
            get => _downloadSpeed;
            set => Set(ref _downloadSpeed, value);
        }

        public long UploadSpeed
        {
            get => _uploadSpeed;
            set => Set(ref _uploadSpeed, value);
        }

        public long PausedTorrentCount
        {
            get => _pausedTorrentCount;
            set => Set(ref _pausedTorrentCount, value);
        }

        public long TorrentCount
        {
            get => _torrentCount;
            set => Set(ref _torrentCount, value);
        }

        public long UploadedBytes
        {
            get => _uploadedBytes;
            set => Set(ref _uploadedBytes, value);
        }

        public long DownloadedBytes
        {
            get => _downloadedBytes;
            set => Set(ref _downloadedBytes, value);
        }

        public long Uptime
        {
            get => _uptime;
            set => Set(ref _uptime, value);
        }

        public long FilesAdded
        {
            get => _filesAdded;
            set => Set(ref _filesAdded, value);
        }

        public long AlternativeSpeedDown
        {
            get => _alternativeSpeedDown;
            set => Set(ref _alternativeSpeedDown, value);
        }

        public bool AlternativeSpeedEnabled
        {
            get => _alternativeSpeedEnabled;
            set => Set(ref _alternativeSpeedEnabled, value);
        }


        public long AlternativeSpeedTimeBegin
        {
            get => _alternativeSpeedTimeBegin;
            set => Set(ref _alternativeSpeedTimeBegin, value);
        }


        public bool AlternativeSpeedTimeEnabled
        {
            get => _alternativeSpeedTimeEnabled;
            set => Set(ref _alternativeSpeedTimeEnabled, value);
        }


        public long AlternativeSpeedTimeEnd
        {
            get => _alternativeSpeedTimeEnd;
            set => Set(ref _alternativeSpeedTimeEnd, value);
        }


        public long AlternativeSpeedTimeDay
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


        public bool BlocklistEnabled
        {
            get => _blocklistEnabled;
            set => Set(ref _blocklistEnabled, value);
        }


        public long CacheSizeMB
        {
            get => _cacheSizeMb;
            set => Set(ref _cacheSizeMb, value);
        }

        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set => Set(ref _downloadDirectory, value);
        }


        public long DownloadQueueSize
        {
            get => _downloadQueueSize;
            set => Set(ref _downloadQueueSize, value);
        }


        public bool DownloadQueueEnabled
        {
            get => _downloadQueueEnabled;
            set => Set(ref _downloadQueueEnabled, value);
        }

        public bool DHTEnabled
        {
            get => _dhtEnabled;
            set => Set(ref _dhtEnabled, value);
        }


        public string Encryption
        {
            get => _encryption;
            set => Set(ref _encryption, value);
        }


        public long IdleSeedingLimit
        {
            get => _idleSeedingLimit;
            set => Set(ref _idleSeedingLimit, value);
        }


        public bool IdleSeedingLimitEnabled
        {
            get => _idleSeedingLimitEnabled;
            set => Set(ref _idleSeedingLimitEnabled, value);
        }


        public string IncompleteDirectory
        {
            get => _incompleteDirectory;
            set => Set(ref _incompleteDirectory, value);
        }


        public bool IncompleteDirectoryEnabled
        {
            get => _incompleteDirectoryEnabled;
            set => Set(ref _incompleteDirectoryEnabled, value);
        }


        public bool LPDEnabled
        {
            get => _lpdEnabled;
            set => Set(ref _lpdEnabled, value);
        }

        public long PeerLimitGlobal
        {
            get => _peerLimitGlobal;
            set => Set(ref _peerLimitGlobal, value);
        }


        public long PeerLimitPerTorrent
        {
            get => _peerLimitPerTorrent;
            set => Set(ref _peerLimitPerTorrent, value);
        }


        public bool PexEnabled
        {
            get => _pexEnabled;
            set => Set(ref _pexEnabled, value);
        }


        public long PeerPort
        {
            get => _peerPort;
            set => Set(ref _peerPort, value);
        }


        public bool PeerPortRandomOnStart
        {
            get => _peerPortRandomOnStart;
            set => Set(ref _peerPortRandomOnStart, value);
        }


        public bool PortForwardingEnabled
        {
            get => _portForwardingEnabled;
            set => Set(ref _portForwardingEnabled, value);
        }


        public bool QueueStalledEnabled
        {
            get => _queueStalledEnabled;
            set => Set(ref _queueStalledEnabled, value);
        }

        public long QueueStalledMinutes
        {
            get => _queueStalledMinutes;
            set => Set(ref _queueStalledMinutes, value);
        }

        public bool RenamePartialFiles
        {
            get => _renamePartialFiles;
            set => Set(ref _renamePartialFiles, value);
        }


        public string ScriptTorrentDoneFilename
        {
            get => _scriptTorrentDoneFilename;
            set => Set(ref _scriptTorrentDoneFilename, value);
        }


        public bool ScriptTorrentDoneEnabled
        {
            get => _scriptTorrentDoneEnabled;
            set => Set(ref _scriptTorrentDoneEnabled, value);
        }


        public double SeedRatioLimit
        {
            get => _seedRatioLimit;
            set => Set(ref _seedRatioLimit, value);
        }


        public bool SeedRatioLimited
        {
            get => _seedRatioLimited;
            set => Set(ref _seedRatioLimited, value);
        }


        public long SeedQueueSize
        {
            get => _seedQueueSize;
            set => Set(ref _seedQueueSize, value);
        }


        public bool SeedQueueEnabled
        {
            get => _seedQueueEnabled;
            set => Set(ref _seedQueueEnabled, value);
        }


        public long SpeedLimitDown
        {
            get => _speedLimitDown;
            set => Set(ref _speedLimitDown, value);
        }

        public bool SpeedLimitDownEnabled
        {
            get => _speedLimitDownEnabled;
            set => Set(ref _speedLimitDownEnabled, value);
        }


        public long SpeedLimitUp
        {
            get => _speedLimitUp;
            set => Set(ref _speedLimitUp, value);
        }


        public bool SpeedLimitUpEnabled
        {
            get => _speedLimitUpEnabled;
            set => Set(ref _speedLimitUpEnabled, value);
        }


        public bool StartAddedTorrents
        {
            get => _startAddedTorrents;
            set => Set(ref _startAddedTorrents, value);
        }

        public bool TrashOriginalTorrentFiles
        {
            get => _trashOriginalTorrentFiles;
            set => Set(ref _trashOriginalTorrentFiles, value);
        }


        public bool UtpEnabled
        {
            get => _utpEnabled;
            set => Set(ref _utpEnabled, value);
        }


        public long BlocklistSize
        {
            get => _blocklistSize;
            set => Set(ref _blocklistSize, value);
        }

        public string ConfigDirectory
        {
            get => _configDirectory;
            set => Set(ref _configDirectory, value);
        }


        public long RpcVersion
        {
            get => _rpcVersion;
            set => Set(ref _rpcVersion, value);
        }


        public long RpcVersionMinimum
        {
            get => _rpcVersionMinimum;
            set => Set(ref _rpcVersionMinimum, value);
        }


        public string Version
        {
            get => _version;
            set => Set(ref _version, value);
        }
    }
}
