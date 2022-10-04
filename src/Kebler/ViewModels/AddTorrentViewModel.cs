using System.Collections.Generic;
using System.Windows.Threading;
using Kebler.Core.Domain.Intrfaces;
using Kebler.Core.Models;
using Kebler.Core.Models.Arguments;
using Kebler.Resources;
using Kebler.TransmissionTorrentClient;
using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.ViewModels
{
    using Caliburn.Micro;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using BencodeNET.Objects;
    using BencodeNET.Parsing;
    using BencodeNET.Torrents;
    using Models;
    using Services;
    using Microsoft.Win32;
    using Models.Interfaces;
    using UI.CSControls.TreeListView;
    using Views;
    using Microsoft.AppCenter.Crashes;
    using System.Text;
    using Kebler.Core.Extensions;
    using NLog;

    public class AddTorrentViewModel : Screen, IHandle<Messages.DownlaodCategoriesChanged>
    {
        #region Properties

        private readonly IEventAggregator _eventAggregator;
        private BindableCollection<FolderCategory> _folderCategory = new BindableCollection<FolderCategory>();
        private Torrent _torrent;
        private object view;
        private ILogger Log = NLog.LogManager.GetCurrentClassLogger();
        private readonly IRemoteTorrentClient _transmissionClient;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        private FileInfo _fileInfo;
        private BindableCollection<string> _dirs;
        private SessionSettings settings;
        public AddTorrentResponse TorrentResult;
        //public bool Result;
        private IReadOnlyCollection<TorrentInfo> _infos;
        private IEnumerable<(string, uint)> _torrents;
        private string _torrentPath, _downlaodDirPath;
        private bool _isWorking, _isAddTorrentWindowShow, _isAutoStart;
        private int _peerLimit, _uploadLimit, _downlaodDirIndex;
        //private ITreeModel _files;
        private Action<uint> _remove;
        private Visibility _resultVisibility = Visibility.Collapsed,
            _loadingGridVisibility = Visibility.Collapsed;

        //private FilesTreeViewModel _filesTree;

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
            set
            {
                Set(ref _isAutoStart, value);
                ConfigService.Instanse.IsAutoStartTorrentAfterAdd = value;
                ConfigService.Save();
            }

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
        public AddTorrentViewModel(string path, IRemoteTorrentClient transmissionClient, SessionSettings settings,
            IReadOnlyCollection<TorrentInfo> infos,
            BindableCollection<FolderCategory> folderCategory, IEventAggregator eventAggregator,
            IEnumerable<(string, uint)> torrents, Action<uint> remove, ref bool isWindOpened, ref KeblerView wnd)
        {
            isWindOpened = true;
            _torrents = torrents;
            _eventAggregator = eventAggregator;
            _eventAggregator?.SubscribeOnPublishedThread(this);
            _remove = remove;
            _wnd = wnd;
            //_wnd?.Activate();

            _infos = infos;
            _fileInfo = new FileInfo(path);
            TorrentPath = _fileInfo.FullName;
            _transmissionClient = transmissionClient;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            IsAutoStart = ConfigService.Instanse.IsAutoStartTorrentAfterAdd;
            this.settings = settings;


            Log.Trace("DownlaodCategoriesChanged");

            var str = new StringBuilder();

            foreach (var item in folderCategory)
            {
                DownlaodDirs.Add(item.FullPath);
                str.Append($"'{item.FullPath}'");
            }

            Log.Trace(str.ToString());

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Need to show window?</returns>
        public async Task<bool> Init()
        {

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;


            DownlaodDirPath = String.IsNullOrEmpty(ConfigService.Instanse.LastUsedLocation) ? DownlaodDirs.FirstOrDefault() : ConfigService.Instanse.LastUsedLocation;

            TorrentResult = new AddTorrentResponse(Enums.ReponseResult.Ok);
            TorrentResult.Value = new TorrentAddResult();

            if (_infos.Any(x => x.HashString.ToLower().Equals(_torrent.OriginalInfoHash.ToLower())))
            {
                Log.Warn("Found torrent with same hash");
                //var info = _infos.First(x => x.HashString.ToLower().Equals(_torrent.OriginalInfoHash.ToLower()));

                //DownlaodDir = info.DownloadDir;
                var manager = IoC.Get<IWindowManager>();

                var quest = LocalizationProvider.GetLocalizedValue(nameof(Strings.ASK_UpdateTrackers));

                var shouldUpdate = (bool?)true;
                if (ConfigService.Instanse.AskUpdateTrackers)
                {
                    Log.Trace("Show dialog for update trackers");
                    shouldUpdate = await MessageBoxViewModel.ShowDialog($"{quest} for {_torrent.DisplayName}", null, null, Enums.MessageBoxDilogButtons.YesNoCancel);
                    Log.Trace($"Dialog for update trackers result => {shouldUpdate}");
                }

                if (shouldUpdate == true)
                {
                    var foundedTorrent = _infos.First(x => x.HashString.ToLower().Equals(_torrent.OriginalInfoHash.ToLower()));

                    LoadingGridVisibility = Visibility.Visible;
                    var toAdd = _torrent.Trackers.Select(tr => tr.First()).ToArray();

                    Log.Trace("Try update trackers");


                    await UpdateTrackers(toAdd, foundedTorrent.Id);
                    //Result = true;

                    TorrentResult.Value.Status = Enums.AddTorrentStatus.UpdatedTrackers;

                    Log.Trace("Update trackers done");

                    await TryCloseAsync(true);
                    return false;
                }
                else if (shouldUpdate == null)
                {
                    TorrentResult.Value.Status = Enums.AddTorrentStatus.UnknownError;

                    await TryCloseAsync(false);
                    return true;
                }
                else
                {
                    TorrentResult.Value.Status = Enums.AddTorrentStatus.UpdatedTrackers;

                    await TryCloseAsync(true);
                    return false;
                }

            }

            return true;
        }



        protected override void OnViewLoaded(object view)
        {
            try
            {
                this.view = view;
                LoadTree();

                _wnd?.Activate();

                if (_wnd?.WindowState == WindowState.Minimized)
                    _wnd.WindowState = WindowState.Normal;

                base.OnViewLoaded(view);
            }
            finally
            {
                LoadingGridVisibility = Visibility.Collapsed;
            }
        }

        public Task<bool> AddHidden()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            return Add();
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

        public async Task<bool> Add()
        {
            ResultVisibility = Visibility.Collapsed;
            IsWorking = true;

            var result = await Task.Run<bool>(async () =>
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
                            PeerLimit = PeerLimit,
                            FilesUnwanted = unWant.ToArray(),
                            FilesWanted = want.ToArray()
                        };

                        if (!string.IsNullOrEmpty(DownlaodDirPath))
                        {
                            newTorrent.DownloadDirectory = DownlaodDirPath;
                        }

                        Log.Info($"Start adding torrentFileInfo {newTorrent.Filename}");
                        Log.Info($"DownloadDirectory set to '{newTorrent.DownloadDirectory}'");

                        while (true)
                        {
                            if (Dispatcher.CurrentDispatcher.HasShutdownStarted)
                                return false;
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

                                                }

                                                ConfigService.Instanse.TorrentPeerLimit = PeerLimit;
                                                ConfigService.Save();
                                                //Result = true;
                                                await TryCloseAsync(true);
                                                break;
                                            }
                                        case Enums.AddTorrentStatus.Duplicate:
                                            {
                                                Log.Info(
                                                    $"Adding torrentFileInfo result '{TorrentResult.Value.Status}' '{TorrentResult.Value.Name}'");

                                                if (_torrent != null)
                                                {
                                                    var toAdd = _torrent.Trackers.Select(tr => tr.First()).ToArray();

                                                    var quest = LocalizationProvider.GetLocalizedValue(
                                                            nameof(Strings.ASK_UpdateTrackers));


                                                    bool? shouldUpdate = true;

                                                    if (ConfigService.Instanse.AskUpdateTrackers)
                                                    {
                                                        shouldUpdate = await MessageBoxViewModel.ShowDialog($"{quest} for {_torrent.DisplayName}", null, null, Enums.MessageBoxDilogButtons.YesNoCancel);
                                                    }

                                                    if (shouldUpdate == true)
                                                    {
                                                        await UpdateTrackers(toAdd, TorrentResult.Value.ID);
                                                    }


                                                    foreach (var tr in toAdd)
                                                    {
                                                        Log.Info($"Tracker added: {tr}'");
                                                    }


                                                    //Result = true;
                                                    await TryCloseAsync(true);
                                                }
                                                break;
                                            }
                                    }

                                    return true;
                                }

                                if (TorrentResult.Result == Enums.ReponseResult.NotOk &&
                                    TorrentResult.CustomException != null)
                                {

                                    await MessageBoxViewModel.ShowDialog(TorrentResult.CustomException.Message
                                        , null, string.Empty);

                                    Log?.Error(TorrentResult.CustomException);

                                    return false;
                                }
                                else
                                {
                                    Log?.Info(
                                        $"Adding torrentFileInfo result '{TorrentResult?.Value?.Status}' '{newTorrent?.Filename}'");
                                    //await Task.Delay(100, cancellationToken);

                                    return true;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex);
                                Crashes.TrackError(ex);

                            }

                            return false;
                        }
                    }
                    else
                    {
                        Log.Error("Could not parse torrent file");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    Crashes.TrackError(ex);

                    return false;
                }
                finally
                {
                    IsWorking = false;
                }
            }, cancellationToken);

            return result;
        }

        private async Task UpdateTrackers(string[] trackers, uint id)
        {
            await _transmissionClient.TorrentSetAsync(new TorrentSettings()
            {
                IDs = new[] { id },
                TrackerAdd = trackers
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
            //Result = false;
            await TryCloseAsync(false);
        }

        private void Closing()
        {
            //_filesTree = null;
            cancellationTokenSource?.Cancel();
            settings = null;
            _fileInfo = null;
        }
        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            if(ConfigService.Instanse.LastUsedLocation != DownlaodDirPath)
            {
                Log.Info("ConfigService.Instanse.LastUsedLocation != DownlaodDirPath");

                Log.Info($"Old '{ConfigService.Instanse.LastUsedLocation}'");

                ConfigService.Instanse.LastUsedLocation = DownlaodDirPath;
                ConfigService.Save();
                Log.Info($"New '{ConfigService.Instanse.LastUsedLocation}'");

            }

            return base.TryCloseAsync(dialogResult);
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

            Log.Trace("DownlaodCategoriesChanged");

            var str = new StringBuilder();

            foreach (var dir in DownlaodDirs)
            {
                str.AppendLine($"'{dir}'" + Environment.NewLine);
            }

            Log.Trace(str.ToString());

            if (string.IsNullOrEmpty(DownlaodDirPath))
            {
                DownlaodDirPath = DownlaodDirs.FirstOrDefault();
            }


            return Task.CompletedTask;
        }
    }
}