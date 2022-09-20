using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Kebler.Core.Domain.Intrfaces;
using Kebler.Core.Models;
using Kebler.Core.Models.Arguments;
using Kebler.Models;
using Kebler.Models.Interfaces;
using Kebler.Services;
using Kebler.TransmissionTorrentClient;
using Kebler.TransmissionTorrentClient.Models;
using Kebler.Views;
using Microsoft.VisualBasic;
using NLog;
using static Kebler.Models.Messages;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Kebler.ViewModels
{
    public class MoreInfoViewModel : PropertyChangedBase
    {
        #region Props

        private static ILogger Log = NLog.LogManager.GetCurrentClassLogger();


        private DateTime _addedOn, _createdOn, _completedOn;
        private long _downloaded, _downloadSpeed, _uploadSpeed, _uploaded, _remaining, _size;

        private string _downloadLimit,
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

        private bool _loading, _isMore;


        private IEnumerable<TransmissionTorrentPeers> _peers = new TransmissionTorrentPeers[0];
        private double _percentDone;
        private int _selectedCount, _maxPeers;
        private IList _selectedTrackers;
        private int _status, _piecesCount;
        private TorrentInfo _ti;
        private Visibility _formsVisibility;
        private bool _isShowMoreInfoCheck;
        private BindableCollection<TransmissionTorrentTrackerStats> _trackerStats =
            new BindableCollection<TransmissionTorrentTrackerStats>();

        private uint[] id;


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

        public IEnumerable<TransmissionTorrentPeers> Peers
        {
            get => _peers;
            set => Set(ref _peers, value);
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
            get => _seeds ?? string.Empty;
            set => Set(ref _seeds, value);
        }

        public string Error
        {
            get => _error ?? string.Empty;
            set => Set(ref _error, value);
        }

        public long UploadSpeed
        {
            get => _uploadSpeed;
            set => Set(ref _uploadSpeed, value);
        }

        public string UploadLimit
        {
            get => _uploadLimit ?? string.Empty;
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

        public bool IsShowMoreInfoCheck
        {
            get => _isShowMoreInfoCheck;
            set
            {
                Set(ref _isShowMoreInfoCheck, value);
                ConfigService.Instanse.MoreInfoShow = !value;
                ConfigService.Save();
                _eventAggregator?.PublishOnUIThreadAsync(new ShowMoreInfoChanged());

                if (value && hide != null)
                {
                    hide(false);
                }
            }
        }

        public bool IsShowMoreInfoCheckNotify
        {
            set
            {
                Set(ref _isShowMoreInfoCheck, value, nameof(IsShowMoreInfoCheck));
            }
        }

        #endregion

        static CancellationTokenSource source = new CancellationTokenSource();
        //static CancellationToken _token;

        KeblerView view;
        static FilesModel model;
        private static IRemoteTorrentClient _client;
        IEventAggregator _eventAggregator;
        Action<bool> hide;
        public MoreInfoViewModel(KeblerView view, Action<bool> unselect, IEventAggregator eventAggregator)
        {
            this.view = view;

            //IsShowMoreInfoCheck = ConfigService.Instanse.MoreInfoShow;
            _eventAggregator = eventAggregator;
            if (unselect != null)
            {
                hide = unselect;
            }
        }


        void Set(TorrentFile trn)
        {
            if (trn.Children.Count > 0)
            {
                foreach (var item in trn.Children)
                {
                    Set(item);
                }
            }
            else
            {
                trn.Done = _ti.FileStats.ToArray()[trn.Index].BytesCompleted;
                if (trn.Done > 0)
                {
                    double p = (trn.Done * 100) / trn.Size;
                    trn.DonePercent = Math.Round(p, 1);
                }
                //trn.checked1 = _ti.FileStats[trn.Index].Wanted;
            }
        }


        public void Update(uint[] ids, IRemoteTorrentClient client, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            Loading = true;
            source.Cancel();
            model = null;
            source = new CancellationTokenSource();
            _client = client;
            id = ids;

            FormsVisibility = Visibility.Collapsed;

            Task.Run(async () =>
            {
                while (!source.IsCancellationRequested)
                {
                    var answ = await _client.TorrentGetAsyncWithId(TorrentFields.ALL_FIELDS, new CancellationToken(), id);
                    if (token.IsCancellationRequested)
                        return;

                    if (answ != null && answ.Torrents.Length == 1)
                    {
                        _ti = answ.Torrents.First();
                        Debug.WriteLine(_ti.Name);
                        if (model == null)
                        {
                            model = FilesModel.CreateTestModel(_ti);
                            OnUIThread(() =>
                            {
                                view.MoreView.FileTreeViewControl.tree.Model = model;
                            });

                        }
                        else
                        {
                            Set(model.Root);
                        }






                        if (_piecesBase64 != _ti.Pieces)
                        {
                            _piecesBase64 = _ti.Pieces;

                            PiecesCount = _ti.PieceCount;
                            PercentDone = _ti.PercentDone;

                            var decodedPices = _piecesBase64.Length > 0 ? Convert.FromBase64CharArray(_piecesBase64.ToCharArray(), 0, _piecesBase64.Length) : new byte[0];
                            OnUIThread(() =>
                            {
                                view.MoreView.Pieces.Init(decodedPices, _ti.PieceCount, DonePices);
                            });
                        }




                        Peers = _ti.Peers;
                        Status = _ti.Status;
                        Downloaded = _ti.DownloadedEver;
                        DownloadSpeed = _ti.RateDownload;
                        DownloadLimit = _ti.DownloadLimited ? _ti.DownloadLimit.ToString() : "-";

                        if (_ti.TrackerStats.Count() > 0)
                        {
                            Seeds =
                                $"{_ti.PeersSendingToUs} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TI_webSeedsOF))}" +
                                $" {_ti.TrackerStats.Max(x => x.SeederCount)} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TI_webSeedsConnected))}";
                            PeersCount =
                                $"{_ti.PeersConnected} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TI_webSeedsOF))}" +
                                $" {_ti.TrackerStats.Max(x => x.LeecherCount)} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TI_webSeedsConnected))}";
                        }

                        Error = Utils.GetErrorString(_ti);
                        Uploaded = _ti.UploadedEver;
                        UploadSpeed = _ti.RateUpload;
                        UploadLimit = _ti.UploadLimited ? _ti.UploadLimit.ToString() : "-";

                        Remaining = _ti.LeftUntilDone;
                        //Wasted = $"({_ti.CorruptEver} {Strings.TI_hashfails})";

                        //var home = Application.Current.Resources;

                        Wasted = $"({_ti.CorruptEver} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TI_hashfails))})";
                        Ratio = $"{_ti.UploadRatio}";
                        MaxPeers = _ti.MaxConnectedPeers;

                        Path = System.IO.Path.Combine(_ti.DownloadDir, _ti.Name);
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
                    await Task.Delay(1500);
                }

            }, source.Token);
        }

        private void DonePices()
        {
            FormsVisibility = Visibility.Visible;
        }

        public void MakeRecheck()
        {
            Task.Run(async () =>
            {
                List<uint> wanted = new List<uint>();
                List<uint> unWanted = new List<uint>();
                var root = ((FilesModel)view.MoreView.FileTreeViewControl.tree.Model).Root;
                var thiId = _ti?.Id;

                if (root != null && thiId != null)
                {
                    void Get(TorrentFile trn)
                    {
                        if (trn.Children.Count > 0)
                        {
                            foreach (var item in trn.Children)
                            {
                                Get(item);
                            }
                        }
                        else
                        {
                            if (trn.Checked == true)
                            {
                                wanted.Add(trn.Index);
                            }
                            else
                            {
                                unWanted.Add(trn.Index);
                            }
                        }
                    }

                    wanted.Clear();
                    unWanted.Clear();
                    Get(root);

                    if (_client != null)
                    {

                        await _client.TorrentSetAsync(new TorrentSettings()
                        {
                            IDs = new uint[] { (uint)thiId },
                            FilesUnwanted = unWanted.ToArray(),
                            FilesWanted = wanted.ToArray()
                        }, new CancellationToken());
                    }
                }
            });
        }





        public async void DeleteTracker()
        {
            var ids = id;
            var trackers = SelectedTrackers.Cast<TransmissionTorrentTrackerStats>().ToList();

            if (trackers.Any())
            {
                var mgr = IoC.Get<IWindowManager>();
                var result = await mgr.ShowDialogAsync(new RemoveListDialogViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.DialogBox_RemoveTracker)),
                    trackers.Select(x => x.announce)));

                if (result == true && _client != null)
                {
                    var addRes = await _client.TorrentSetAsync(new TorrentSettings
                    {
                        IDs = ids,
                        TrackerRemove = trackers.Select(x => x.ID).ToArray()
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
                        else if (addRes.WebException != null)
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

            var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.AddTrackerTitile)), string.Empty, false);
            var result = await mgr.ShowDialogAsync(dialog);

            if (result == true && _client != null)
            {
                var addRes = await _client.TorrentSetAsync(new TorrentSettings
                {
                    IDs = ids,
                    TrackerAdd = new[] { dialog.Value.ToString() }
                }, new CancellationToken());

                if (addRes.Success)
                {
                    Log.Info($"Tracker added: {dialog.Value}");
                }
                else
                {
                    if (addRes.CustomException != null)
                        Log.Error(addRes.CustomException);
                    else if (addRes.WebException != null)
                        Log.Error(addRes.WebException);
                    else
                        Log.Error("Unknown error while addind tracker");

                }
            }
        }
    }


    public class FilesModel : ITreeModel
    {

        public static FilesModel CreateTestModel(TorrentInfo ti)
        {
            var model = new FilesModel();
            var p = new TorrentFile() { Name = ti.Name };


            var counter = 0U;
            foreach (var item in ti.Files)
            {
                createNodes(ref p, item.NameParts, item.Length, item.BytesCompleted, ti.FileStats.ToArray()[counter].Wanted, counter);

                counter++;
            }


            model.Root.Children.Add(p.Children.First());
            return model;
        }

        public static FilesModel CreateTestModel(BencodeNET.Torrents.Torrent ti)
        {
            var model = new FilesModel();
            var p = new TorrentFile() { Name = ti.DisplayName };


            if (ti.Files != null)
            {
                var counter = 0U;
                foreach (var item in ti.Files)
                {
                    createNodes(ref p, item.Path.ToArray(), item.FileSize, 0, true, counter);

                    counter++;
                }

                model.Root.Children.Add(p);
            }
            else if (ti.File != null)
            {
                var node = new TorrentFile(ti.File.FileName, ti.File.FileSize, 0, true, 0);
                model.Root.Children.Add(node);
            }


            return model;
        }


        private static void createNodes(ref TorrentFile root, string[] file, long size, long done, bool check, uint index)
        {
            var last = root;

            for (var i = 0; i < file.Length; i++)
            {
                var pathPart = file[i];
                if (string.IsNullOrEmpty(pathPart))
                    continue;

                if (last.Children.All(x => x.Name != pathPart))
                {
                    //if not found children
                    if (i + 1 == file.Length)
                    {
                        var pth = new TorrentFile(pathPart, size, done, check, index);
                        pth.Parent = last;
                        last.Children.Add(pth);
                        last = pth;
                        RecalParent(last);
                    }
                    else
                    {
                        var pth = new TorrentFile(pathPart);
                        pth.Parent = last;
                        last.Children.Add(pth);
                        last = pth;
                    }
                }
                else
                {
                    last = last.Children.First(p => p.Name == pathPart);
                }
            }
        }

        static void RecalParent(TorrentFile trn)
        {
            if (trn.Parent != null)
            {
                //allFalse
                if (trn.Parent.Children.All(x => x.Checked == false))
                {
                    trn.Parent.Checked = false;
                }
                //allTrue
                else if (trn.Parent.Children.All(x => x.Checked == true))
                {
                    trn.Parent.Checked = true;
                }
                else
                {
                    trn.Parent.Checked = null;
                }
                RecalParent(trn.Parent);
            }
        }


        public TorrentFile Root { get; private set; }
        public FilesModel()
        {
            Root = new TorrentFile();
        }


        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                parent = Root;
            return (parent as TorrentFile).Children;
        }

        public bool HasChildren(object parent)
        {
            return (parent as TorrentFile).Children.Count > 0;
        }


    }



}