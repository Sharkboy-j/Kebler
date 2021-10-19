using System.Collections.Generic;
using System.Windows.Threading;
using Kebler.Models.Torrent;
using Kebler.Resources;

namespace Kebler.ViewModels
{
    using Caliburn.Micro;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using BencodeNET.Objects;
    using BencodeNET.Parsing;
    using BencodeNET.Torrents;
    using Models;
    using Models.Torrent.Args;
    using Models.Torrent.Response;
    using Services;
    using TransmissionCore;
    using Microsoft.Win32;
    using ILog = log4net.ILog;
    using LogManager = log4net.LogManager;
    using Models.Interfaces;
    using UI.CSControls.TreeListView;
    using Views;
    using Microsoft.AppCenter.Crashes;

    public class AddTorrentViewModel : Screen, IHandle<Messages.DownlaodCategoriesChanged>
    {
        #region Properties

        private readonly IEventAggregator? _eventAggregator;
        private BindableCollection<FolderCategory> _folderCategory = new BindableCollection<FolderCategory>();
        private Torrent _torrent;
        private object view;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TransmissionClient _transmissionClient;
        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        private FileInfo? _fileInfo;
        private BindableCollection<string> _dirs;
        private SessionSettings? settings;
        public AddTorrentResponse TorrentResult;
        public bool Result;
        private IEnumerable<TorrentInfo> _infos;
        private IEnumerable<(string, uint)> _torrents;
        private string? _torrentPath, _downlaodDirPath;
        private bool _isWorking, _isAddTorrentWindowShow, _isAutoStart;
        private int _peerLimit, _uploadLimit, _downlaodDirIndex;
        private ITreeModel _files;
        private Action<uint> _remove;
        private Visibility _resultVisibility = Visibility.Collapsed,
            _loadingGridVisibility = Visibility.Collapsed;

        //private FilesTreeViewModel? _filesTree;

        // ReSharper disable once MemberCanBePrivate.Global
        //public FilesTreeViewModel FilesTree
        //{
        //    // ReSharper disable once UnusedMember.Global
        //    get => _filesTree ??= new FilesTreeViewModel();
        //    set => Set(ref _filesTree, value);
        //}


        public string TorrentPath
        {
            get => _torrentPath ??= string.Empty;
            set => Set(ref _torrentPath, value);
        }

        public BindableCollection<string> DownlaodDirs
        {
            get
            {
                if (_dirs == null)
                    _dirs = new BindableCollection<string>();
                return _dirs;
            }
            set => Set(ref _dirs, value);
        }

        public string DownlaodDirPath
        {
            get => _downlaodDirPath ??= string.Empty;
            set => Set(ref _downlaodDirPath, value);
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => Set(ref _isWorking, value);
        }

        public bool IsAddTorrentWindowShow
        {
            get => _isAddTorrentWindowShow;
            set => Set(ref _isAddTorrentWindowShow, value);
        }

        public int PeerLimit
        {
            get => _peerLimit;
            set => Set(ref _peerLimit, value);
        }

        public int UploadLimit
        {
            get => _uploadLimit;
            set => Set(ref _uploadLimit, value);
        }

        public bool IsAutoStart
        {
            get => _isAutoStart;
            set => Set(ref _isAutoStart, value);
        }

        public Visibility ResultVisibility
        {
            get => _resultVisibility;
            set => Set(ref _resultVisibility, value);
        }

        public Visibility LoadingGridVisibility
        {
            get => _loadingGridVisibility;
            set => Set(ref _loadingGridVisibility, value);
        }

        public int DownlaodDirIndex
        {
            get => _downlaodDirIndex;
            set => Set(ref _downlaodDirIndex, value);
        }

        #endregion
        KeblerView _wnd;
        public AddTorrentViewModel(string path, TransmissionClient? transmissionClient, SessionSettings? settings,
            IEnumerable<TorrentInfo> infos,
            BindableCollection<FolderCategory> folderCategory, IEventAggregator? eventAggregator,
            IEnumerable<(string, uint)> torrents, Action<uint> remove, ref bool isWindOpened, ref KeblerView wnd)
        {
            isWindOpened = true;
            _torrents = torrents;
            _eventAggregator = eventAggregator;
            _eventAggregator?.SubscribeOnPublishedThread(this);
            _remove = remove;
            _wnd = wnd;
            _wnd?.Activate();

            if (_wnd?.WindowState == WindowState.Minimized)
                _wnd.WindowState = WindowState.Normal;

            _infos = infos;
            _fileInfo = new FileInfo(path);
            TorrentPath = _fileInfo.FullName;
            _transmissionClient = transmissionClient;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            IsAutoStart = true;
            this.settings = settings;


            foreach (var item in folderCategory)
            {
                DownlaodDirs.Add(item.FullPath);
            }

            DownlaodDirIndex = 0;

            if (!ConfigService.Instanse.IsAddTorrentWindowShow)
            {
                IsWorking = false;
                IsAddTorrentWindowShow = ConfigService.Instanse.IsAddTorrentWindowShow;
            }

            PeerLimit = ConfigService.Instanse.TorrentPeerLimit;
            //FilesTree.Border = new Thickness(1);

            using var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _torrent = new BencodeParser().Parse<Torrent>(file);
        }

        protected override async void OnViewLoaded(object view)
        {
            try
            {
                this.view = view;
                LoadTree();
                base.OnViewLoaded(view);
            }
            finally
            {
                LoadingGridVisibility = Visibility.Collapsed;
            }

            if (_infos.Any(x => x.HashString.ToLower().Equals(_torrent.OriginalInfoHash.ToLower())))
            {
                var info = _infos.First(x => x.HashString.ToLower().Equals(_torrent.OriginalInfoHash.ToLower()));

                //DownlaodDir = info.DownloadDir;
                var manager = IoC.Get<IWindowManager>();
                if (await MessageBoxViewModel.ShowDialog(
                    LocalizationProvider.GetLocalizedValue(nameof(Strings
                        .ATD_TorrentExist_UpdateTrackers))
                    , manager, string.Empty, Enums.MessageBoxDilogButtons.YesNo) == true)
                {
                    LoadingGridVisibility = Visibility.Visible;
                    Add();
                    await TryCloseAsync(true);
                }
                else
                {
                    await TryCloseAsync(false);
                }
            }
        }


        public void ChangeVisibilityWindow()
        {
            ConfigService.Instanse.IsAddTorrentWindowShow = IsAddTorrentWindowShow;
            ConfigService.Save();
        }

        private void LoadTree()
        {
            LoadingGridVisibility = Visibility.Visible;

            try
            {
                if (_torrent != null)
                {
                    if (view is IAddTorrent _addView && _addView.FilesTreeView is TreeListView vv)
                        vv.Model = FilesModel.CreateTestModel(_torrent);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Crashes.TrackError(ex);
            }
        }

        public void ChangePath()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Torrent files (*.torrent)|*.torrent|All files (*.*)|*.*",
                    Multiselect = false,
                    InitialDirectory = _fileInfo?.DirectoryName ?? throw new InvalidOperationException()
                };

                if (openFileDialog.ShowDialog() != true) return;

                _fileInfo = new FileInfo(openFileDialog.FileName);
                TorrentPath = _fileInfo.FullName;


                using var file = File.Open(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                _torrent = new BencodeParser().Parse<Torrent>(file);
                LoadTree();
            }
            finally
            {
                LoadingGridVisibility = Visibility.Collapsed;
            }
        }

        private void merge(ref uint[] array1, ref uint[] array2, out uint[] result)
        {
            var array1OriginalLength = array1.Length;
            Array.Resize(ref array1, array1OriginalLength + array2.Length);
            Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
            result = array1;
        }

        private uint[] get(TorrentFile Node, bool search)
        {
            var output = new uint[0];
            if (Node.Children.Count > 0)
            {
                foreach (var item in Node.Children)
                    if (item.Children.Count > 0)
                    {
                        var ids = get(item, search);
                        merge(ref output, ref ids, out output);
                    }
                    else
                    {
                        if (item.Checked == search)
                        {
                            Array.Resize(ref output, output.Length + 1);
                            output[output.GetUpperBound(0)] = item.Index;
                            //dd.Add(item.IndexPattern);
                        }
                    }
            }
            else
            {
                if (Node.Checked == search)
                {
                    Array.Resize(ref output, output.Length + 1);
                    output[output.GetUpperBound(0)] = Node.Index;
                    //dd.Add(Node.IndexPattern);
                }
            }

            return output;
        }

        public async void Add()
        {
            ResultVisibility = Visibility.Collapsed;
            IsWorking = true;

            await Task.Run(async () =>
            {
                try
                {
                    //var dd = FilesTree.getFilesWantedStatus(false);
                    List<uint> want = new List<uint>();
                    List<uint> unWant = new List<uint>();

                    if (view is IAddTorrent _addView && _addView.FilesTreeView is TreeListView vv)
                    {
                        var mod = (vv.Model as FilesModel);

                        want = get(mod.Root, true).ToList();
                        unWant = get(mod.Root, false).ToList();
                    }

                    var torrentBytes = _torrent.EncodeAsBytes();

                    if (torrentBytes != null)
                    {
                        var newTorrent = new NewTorrent
                        {
                            Metainfo = Convert.ToBase64String(torrentBytes),
                            Paused = !IsAutoStart,
                            DownloadDirectory = DownlaodDirPath,
                            PeerLimit = PeerLimit,
                            FilesUnwanted = unWant.ToArray(),
                            FilesWanted = want.ToArray()
                        };

                        Log.Info($"Start adding torrentFileInfo {newTorrent.Filename}");

                        while (true)
                        {
                            if (Dispatcher.CurrentDispatcher.HasShutdownStarted) return;
                            try
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var parser = new BencodeParser();
                                var torrent = parser.Parse<Torrent>(torrentBytes);
                                TorrentResult =
                                    await _transmissionClient.TorrentAddAsync(newTorrent, cancellationToken);

                                if (TorrentResult.Result == Enums.ReponseResult.Ok)
                                {
                                    switch (TorrentResult.Value.Status)
                                    {
                                        case Enums.AddTorrentStatus.Added:
                                            {
                                                Log.Info(
                                                    $"Torrent {newTorrent.Filename} added as '{TorrentResult.Value.Name}'");
                                                IsWorking = false;


                                                if (ConfigService.Instanse.CanSearchForSimilatTorrentsWhenAddingNewOne)
                                                {
                                                    Log.Info($"Search for similar");

                                                    await _wnd.Dispatcher.InvokeAsync(async () =>
                                                    {
                                                        if (_torrents.Select(x => x.Item1).Contains(torrent.DisplayName))
                                                        {
                                                            Log.Info($"Here is similar torrent");

                                                            var title = LocalizationProvider.GetLocalizedValue(
                                                                    nameof(Strings.ASK_REMOVE_SIMILAR)).Replace("%d", torrent.DisplayName);
                                                            var res = await MessageBoxViewModel.ShowDialog(title,
                                                                null, null, Enums.MessageBoxDilogButtons.YesNo);
                                                            Log.Info($"Response for removing similar => {res}");

                                                            if (res == true)
                                                            {
                                                                _remove(_torrents.First(x => x.Item1.Equals(torrent.DisplayName)).Item2);
                                                            }
                                                        }

                                                    });

                                                }

                                                ConfigService.Instanse.TorrentPeerLimit = PeerLimit;
                                                ConfigService.Save();
                                                Result = true;
                                                await TryCloseAsync();
                                                return;
                                            }
                                        case Enums.AddTorrentStatus.Duplicate:
                                            {
                                                Log.Info(
                                                    $"Adding torrentFileInfo result '{TorrentResult.Value.Status}' '{TorrentResult.Value.Name}'");

                                                if (_torrent != null)
                                                {
                                                    var toAdd = _torrent.Trackers.Select(tr => tr.First()).ToArray();




                                                    await _wnd.Dispatcher.InvokeAsync(async () =>
                                                    {
                                                        var result = await MessageBoxViewModel.ShowDialog(
                                                            LocalizationProvider.GetLocalizedValue(
                                                            nameof(Strings.ASK_UpdateTrackers))
                                                            , null, null, Enums.MessageBoxDilogButtons.YesNo);

                                                        if (result == true)
                                                        {

                                                            await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                                                            {
                                                                IDs = new[] { TorrentResult.Value.ID },
                                                                TrackerAdd = toAdd
                                                            }, cancellationToken);
                                                        }

                                                    });



                                                    foreach (var tr in toAdd)
                                                    {
                                                        Log.Info($"Tracker added: {tr}'");
                                                    }


                                                    Result = true;
                                                    await TryCloseAsync();
                                                }

                                                return;
                                            }
                                    }
                                }
                                else
                                {
                                    Log?.Info(
                                        $"Adding torrentFileInfo result '{TorrentResult?.Value?.Status}' '{newTorrent?.Filename}'");
                                    await Task.Delay(100, cancellationToken);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                                Crashes.TrackError(ex);

                                return;
                            }
                        }
                    }

                    else
                    {
                        Log.Error("Could not parse torrent file");
                    }


                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Crashes.TrackError(ex);
                }
                finally
                {
                    IsWorking = false;
                }
            }, cancellationToken);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            IsWorking = false;
        }

        public async Task Cancel()
        {
            IsWorking = false;
            cancellationTokenSource.Cancel();
            Result = false;
            await TryCloseAsync();
        }

        private void Closing()
        {
            //_filesTree = null;
            cancellationTokenSource?.Cancel();
            settings = null;
            _fileInfo = null;
        }

        public Task HandleAsync(Messages.DownlaodCategoriesChanged message, CancellationToken cancellationToken)
        {
            var cats = message.Paths.Select(x => x.FullPath);
            var toRm = DownlaodDirs.Except(cats).ToList();
            var toAdd = cats.Except(DownlaodDirs).ToList();

            if (toRm.Any())
            {
                foreach (var itm in toRm) DownlaodDirs.Remove(itm);
            }

            if (toAdd.Any())
            {
                foreach (var itm in toAdd)
                    DownlaodDirs.Add(itm);
            }

            return Task.CompletedTask;
        }
    }
}