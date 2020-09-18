using System;
using System.Linq;
using Caliburn.Micro;
using Kebler.Models.Torrent;
using Kebler.Resources;
using Kebler.Services;

namespace Kebler.ViewModels
{
    public class MoreInfoViewModel : PropertyChangedBase
    {
    


        public void Update(TorrentInfo torrent)
        {
            _ti = torrent;
            FilesTree.UpdateFilesTree(_ti);
            TrackerStats = new BindableCollection<TransmissionTorrentTrackerStats>(_ti.TrackerStats);

            Pieces = _ti.Pieces;
            PiecesCount = _ti.PieceCount;
            PercentDone = _ti.PercentDone;


            Peers = _ti.Peers;
            Status = _ti.Status;
            Downloaded = _ti.DownloadedEver;
            DownloadSpeed = _ti.RateDownload;
            DownloadLimit = _ti.DownloadLimited ? _ti.DownloadLimit.ToString() : "-";
            Seeds = $"{_ti.WebseedsSendingToUs} {Strings.TI_webSeedsOF} {_ti.TrackerStats.Max(x => x.SeederCount)} {Strings.TI_webSeedsConnected}";
            PeersCount = $"{_ti.PeersConnected} {Strings.TI_webSeedsOF} {_ti.TrackerStats.Max(x => x.LeecherCount)} {Strings.TI_webSeedsConnected}";

            Error = Utils.GetErrorString(_ti);
            Uploaded = _ti.UploadedEver;
            UploadSpeed = _ti.RateUpload;
            UploadLimit = _ti.UploadLimited ? _ti.UploadLimit.ToString() : "-";

            Remaining = _ti.LeftUntilDone;
            Wasted = $"({_ti.CorruptEver} {Strings.TI_hashfails})";
            Ratio = $"{_ti.UploadRatio}";
            MaxPeers = _ti.MaxConnectedPeers;

            Path = $"{_ti.DownloadDir}{_ti.Name}";
            Size = _ti.TotalSize;
            Hash = _ti.HashString;
            AddedOn = DateTimeOffset.FromUnixTimeSeconds(_ti.AddedDate).UtcDateTime.ToLocalTime();
            MagnetLink = _ti.MagnetLink;

            CreatedOn = DateTimeOffset.FromUnixTimeSeconds(_ti.DateCreated).UtcDateTime.ToLocalTime();
            CompletedOn = DateTimeOffset.FromUnixTimeSeconds(_ti.DoneDate).UtcDateTime.ToLocalTime();
            Comment = _ti.Comment;
            PiecesString = $"{_ti.PieceCount} x {Utils.GetSizeString(_ti.PieceSize,true)}";


        }

        public uint[] id;
        private FilesTreeViewModel _filesTree = new FilesTreeViewModel();

        private BindableCollection<TransmissionTorrentTrackerStats> _trackerStats =
            new BindableCollection<TransmissionTorrentTrackerStats>();
        private TransmissionTorrentPeers[] _peers = new TransmissionTorrentPeers[0];
        private bool _loading, _isMore;
        private double _percentDone;
        private int _selectedCount, _maxPeers;
        private TorrentInfo _ti;
        private int _status, _piecesCount;
        private long _downloaded, _downloadSpeed, _uploadSpeed, _uploaded, _remaining, _size;
        private string _downloadLimit, _uploadLimit, _seeds, _error, _peersCount, _wasted, _ratio,
            _path, _hash, _magnet, _comment, _pieces, _piecesString;

        private DateTime _addedOn,_createdOn, _completedOn;

        public void DeleteTracker()
        {
            var selectedItems = TrackerStats.Where(i => i.IsSelected).ToList();
        }

        public BindableCollection<TransmissionTorrentTrackerStats> TrackerStats
        {
            get => _trackerStats;
            set => Set(ref _trackerStats, value);
        }

        public TransmissionTorrentPeers[] Peers
        {
            get => _peers;
            set => Set(ref _peers, value);
        }

        public FilesTreeViewModel FilesTree
        {
            get => _filesTree;
            set => Set(ref _filesTree, value);
        }
        public bool Loading
        {
            get => _loading;
            set => Set(ref _loading, value);
        }
        public bool IsMore
        {
            get => _isMore;
            set => Set(ref _isMore, value);
        }
        public int SelectedCount
        {
            get => _selectedCount;
            set => Set(ref _selectedCount, value);
        }
        public double PercentDone
        {
            get => _percentDone;
            set => Set(ref _percentDone, value);
        }

        public void Clear()
        {
            FilesTree.Files = null;
        }

        public int Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public long Downloaded
        {
            get => _downloaded;
            set => Set(ref _downloaded, value);
        }

        public long Uploaded
        {
            get => _uploaded;
            set => Set(ref _uploaded, value);
        }


        public long DownloadSpeed
        {
            get => _downloadSpeed;
            set => Set(ref _downloadSpeed, value);
        }

        public string DownloadLimit
        {
            get => _downloadLimit;
            set => Set(ref _downloadLimit, value);
        }

        public string Seeds
        {
            get => _seeds;
            set => Set(ref _seeds, value);
        }

        public string Error
        {
            get => _error;
            set => Set(ref _error, value);
        }
        public long UploadSpeed
        {
            get => _uploadSpeed;
            set => Set(ref _uploadSpeed, value);
        }

        public string UploadLimit
        {
            get => _uploadLimit;
            set => Set(ref _uploadLimit, value);
        }

        public string PeersCount
        {
            get => _peersCount;
            set => Set(ref _peersCount, value);
        }

        public long Remaining
        {
            get => _remaining;
            set => Set(ref _remaining, value);
        }

        public string Wasted
        {
            get => _wasted;
            set => Set(ref _wasted, value);
        }

        public string Ratio
        {
            get => _ratio;
            set => Set(ref _ratio, value);
        }

        public int MaxPeers
        {
            get => _maxPeers;
            set => Set(ref _maxPeers, value);
        }

        public string Path
        {
            get => _path;
            set => Set(ref _path, value);
        }


        public long Size
        {
            get => _size;
            set => Set(ref _size, value);
        }

        public string Hash
        {
            get => _hash;
            set => Set(ref _hash, value);
        }

        public DateTime AddedOn
        {
            get => _addedOn;
            set => Set(ref _addedOn, value);
        }

        public string MagnetLink
        {
            get => _magnet;
            set => Set(ref _magnet, value);
        }

        public DateTime CreatedOn
        {
            get => _createdOn;
            set => Set(ref _createdOn, value);
        }
        public DateTime CompletedOn
        {
            get => _completedOn;
            set => Set(ref _completedOn, value);
        }


        public string Pieces
        {
            get => _pieces;
            set => Set(ref _pieces, value);
        }


        public string Comment
        {
            get => _comment;
            set => Set(ref _comment, value);
        }


        public int PiecesCount
        {
            get => _piecesCount;
            set => Set(ref _piecesCount, value);
        }
        public string PiecesString
        {
            get => _piecesString;
            set => Set(ref _piecesString, value);
        }
    }
}
