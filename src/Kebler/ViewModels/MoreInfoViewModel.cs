using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Services;
using Kebler.TransmissionCore;
using Kebler.Views;
using Microsoft.VisualBasic;
using Strings = Kebler.Resources.Strings;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Kebler.ViewModels
{
    public class MoreInfoViewModel : PropertyChangedBase
    {
        #region Props

          private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private static TransmissionClient? _client;

        private DateTime _addedOn, _createdOn, _completedOn;
        private long _downloaded, _downloadSpeed, _uploadSpeed, _uploaded, _remaining, _size;

        private string? _downloadLimit,
            _uploadLimit,
            _seeds,
            _error,
            _peersCount,
            _wasted,
            _ratio,
            _path,
            _hash,
            _magnet,
            _comment,
            _piecesBase64,
            _piecesString;

        private FilesTreeViewModel _filesTree = new FilesTreeViewModel();
        private bool _loading, _isMore;


        private TransmissionTorrentPeers[] _peers = new TransmissionTorrentPeers[0];
        private double _percentDone;
        private int _selectedCount, _maxPeers;
        private IList? _selectedTrackers;
        private int _status, _piecesCount;
        private TorrentInfo? _ti;
        private Visibility _formsVisibility;

        private BindableCollection<TransmissionTorrentTrackerStats> _trackerStats =
            new BindableCollection<TransmissionTorrentTrackerStats>();

        private uint[]? id;


        public IList SelectedTrackers
        {
            get => _selectedTrackers ?? new Collection();
            set => Set(ref _selectedTrackers, value);
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
            get => _downloadLimit ?? string.Empty;
            set => Set(ref _downloadLimit, value);
        }

        public string Seeds
        {
            get => _seeds?? string.Empty;
            set => Set(ref _seeds, value);
        }

        public string Error
        {
            get => _error?? string.Empty;
            set => Set(ref _error, value);
        }

        public long UploadSpeed
        {
            get => _uploadSpeed;
            set => Set(ref _uploadSpeed, value);
        }

        public string UploadLimit
        {
            get => _uploadLimit?? string.Empty;
            set => Set(ref _uploadLimit, value);
        }

        public string PeersCount
        {
            get => _peersCount ?? string.Empty;
            set => Set(ref _peersCount, value);
        }

        public long Remaining
        {
            get => _remaining;
            set => Set(ref _remaining, value);
        }

        public string Wasted
        {
            get => _wasted ?? string.Empty;
            set => Set(ref _wasted, value);
        }

        public string Ratio
        {
            get => _ratio ?? string.Empty;
            set => Set(ref _ratio, value);
        }

        public int MaxPeers
        {
            get => _maxPeers;
            set => Set(ref _maxPeers, value);
        }

        public string Path
        {
            get => _path ?? string.Empty;
            set => Set(ref _path, value);
        }


        public long Size
        {
            get => _size;
            set => Set(ref _size, value);
        }

        public string Hash
        {
            get => _hash ?? string.Empty;
            set => Set(ref _hash, value);
        }

        public DateTime AddedOn
        {
            get => _addedOn;
            set => Set(ref _addedOn, value);
        }

        public string MagnetLink
        {
            get => _magnet ?? string.Empty;
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
            get => _piecesBase64 ?? string.Empty;
            set => Set(ref _piecesBase64, value);
        }


        public string Comment
        {
            get => _comment ?? string.Empty;
            set => Set(ref _comment, value);
        }


        public int PiecesCount
        {
            get => _piecesCount;
            set => Set(ref _piecesCount, value);
        }

        public string PiecesString
        {
            get => _piecesString ?? string.Empty;
            set => Set(ref _piecesString, value);
        }

        public Visibility FormsVisibility
        {
            get => _formsVisibility;
            set => Set(ref _formsVisibility, value);
        }

        #endregion


        KeblerView view;

        public MoreInfoViewModel(KeblerView view)
        {
           this.view = view;
        }

        public async Task Update(uint[] ids, TransmissionClient client)
        {
            _client = client;
            this.id = ids;


            Loading = true;
            FormsVisibility = Visibility.Collapsed;

            var answ = await client.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS, new CancellationToken(), id);

            if(answ!=null && answ.Torrents.Length==1)
            {
                _ti = answ.Torrents.First();
                FilesTree.UpdateFilesTree(_ti);

                if (_piecesBase64 != _ti.Pieces)
                {
                    _piecesBase64 = _ti.Pieces;

                    PiecesCount = _ti.PieceCount;
                    PercentDone = _ti.PercentDone;

                    var decodedPices = _piecesBase64.Length > 0 ? Convert.FromBase64CharArray(_piecesBase64.ToCharArray(), 0, _piecesBase64.Length) : new byte[0];
                    view.MoreView.Pieces.Init(decodedPices, _ti.PieceCount, DonePices);
                }




                Peers = _ti.Peers;
                Status = _ti.Status;
                Downloaded = _ti.DownloadedEver;
                DownloadSpeed = _ti.RateDownload;
                DownloadLimit = _ti.DownloadLimited ? _ti.DownloadLimit.ToString() : "-";

                if (_ti.TrackerStats.Length > 0)
                {
                    Seeds =
                        $"{_ti.PeersSendingToUs} {Strings.TI_webSeedsOF} {_ti.TrackerStats.Max(x => x.SeederCount)} {Strings.TI_webSeedsConnected}";
                    PeersCount =
                        $"{_ti.PeersConnected} {Strings.TI_webSeedsOF} {_ti.TrackerStats.Max(x => x.LeecherCount)} {Strings.TI_webSeedsConnected}";
                }

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
                PiecesString = $"{_ti.PieceCount} x {Utils.GetSizeString(_ti.PieceSize, true)}";

                TrackerStats = new BindableCollection<TransmissionTorrentTrackerStats>(_ti.TrackerStats);
            }
            Loading = false;
        }

        private void DonePices()
        {
            FormsVisibility = Visibility.Visible;
        }

        public async void DeleteTracker()
        {
            var ids = id;
            var trackers = SelectedTrackers.Cast<TransmissionTorrentTrackerStats>().ToList();

            if (trackers.Any())
            {
                var mgr = IoC.Get<IWindowManager>();
                var result = await mgr.ShowDialogAsync(new RemoveListDialogViewModel(Strings.DialogBox_RemoveTracker,
                    trackers.Select(x => x.announce)));

                if (result == true && _client != null)
                {
                    var addRes =   await _client.TorrentSetAsync(new TorrentSettings
                    {
                        IDs = ids,
                        TrackerRemove = trackers.Select(x=>x.ID).ToArray()
                    }, new CancellationToken());

                    if (addRes.Success)
                    {
                        foreach (var trac in trackers)
                        {
                            Log.Info($"Tracker removed: {trac.announce}");
                        }
                        TrackerStats.RemoveRange(trackers);
                    }
                    else
                    {
                        if (addRes.CustomException != null)
                            Log.Error(addRes.CustomException);
                        else if(addRes.WebException!=null)
                            Log.Error(addRes.WebException);
                        else 
                            Log.Error("Unknown error while addind tracker");

                    }
                }
            }
        }

        public async void AddTracker()
        {
            var ids = id;
            var mgr = IoC.Get<IWindowManager>();

            var dialog = new DialogBoxViewModel(Strings.AddTrackerTitile, string.Empty, false);
            var result = await mgr.ShowDialogAsync(dialog);

            if (result == true && _client != null)
            {
                var addRes = await _client.TorrentSetAsync(new TorrentSettings
                {
                    IDs = ids,
                    TrackerAdd = new[] {dialog.Value}
                }, new CancellationToken());

                if (addRes.Success)
                {
                    Log.Info($"Tracker added: {dialog.Value}");
                }
                else
                {
                    if (addRes.CustomException != null)
                        Log.Error(addRes.CustomException);
                    else if(addRes.WebException!=null)
                        Log.Error(addRes.WebException);
                    else 
                        Log.Error("Unknown error while addind tracker");

                }
            }
        }

        public void Clear()
        {
            FilesTree.Files = null;
        }
    }
}