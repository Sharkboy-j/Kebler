using System.Windows.Threading;

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

    public class AddTorrentViewModel : Screen
    {
        #region Properties

        private Torrent? _torrent;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TransmissionClient _transmissionClient;
        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        private FileInfo? _torrentFileInfo;
        private SessionSettings? settings;
        public AddTorrentResponse TorrentResult;
        public bool Result;
        
        
        private string? _torrentPath, _downlaodDir;
        private bool _isWorking, _isAddTorrentWindowShow, _isAutoStart;
        private int _peerLimit, _uploadLimit;

        private Visibility _resultVisibility = Visibility.Collapsed,
            _loadingGridVisibility = Visibility.Collapsed;

        private FilesTreeViewModel? _filesTree;

        // ReSharper disable once MemberCanBePrivate.Global
        public FilesTreeViewModel FilesTree
        {
            // ReSharper disable once UnusedMember.Global
            get => _filesTree ??= new FilesTreeViewModel();
            set => Set(ref _filesTree, value);
        }

        public string TorrentPath
        {
            get => _torrentPath ??= string.Empty;
            set => Set(ref _torrentPath, value);
        }

        public string DownlaodDir
        {
            get => _downlaodDir ??= string.Empty;
            set => Set(ref _downlaodDir, value);
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

        #endregion

        public AddTorrentViewModel(string path, TransmissionClient? transmissionClient, SessionSettings? settings)
        {
            _torrentFileInfo = new FileInfo(path);
            TorrentPath = _torrentFileInfo.FullName;
            _transmissionClient = transmissionClient;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            IsAutoStart = true;
            this.settings = settings;

            DownlaodDir = settings.DownloadDirectory;

            if (!ConfigService.Instanse.IsAddTorrentWindowShow)
            {
                IsWorking = false;
                IsAddTorrentWindowShow = ConfigService.Instanse.IsAddTorrentWindowShow;
            }

            PeerLimit = ConfigService.Instanse.TorrentPeerLimit;
            FilesTree.Border = new Thickness(1);

            using var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _torrent = new BencodeParser().Parse<Torrent>(file);
        }

        protected override void OnViewLoaded(object view)
        {
            try
            {
                LoadTree();
                base.OnViewLoaded(view);
            }
            finally
            {
                LoadingGridVisibility = Visibility.Collapsed;
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
                    FilesTree?.Clear();
                    FilesTree?.UpdateFilesTree(_torrent);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
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
                    InitialDirectory = _torrentFileInfo?.DirectoryName ?? throw new InvalidOperationException()
                };

                if (openFileDialog.ShowDialog() != true) return;
            
                _torrentFileInfo = new FileInfo(openFileDialog.FileName);
                TorrentPath = _torrentFileInfo.FullName;
                LoadTree();
            }
            finally
            {
                LoadingGridVisibility = Visibility.Collapsed;
            }
          

        }
        
        public void Add()
        {
            ResultVisibility = Visibility.Collapsed;
            IsWorking = true;

            Task.Run(async () =>
            {
                try
                {
                    //var dd = FilesTree.getFilesWantedStatus(false);

                    var newTorrent = new NewTorrent
                    {
                        Metainfo = Convert.ToBase64String(_torrent.EncodeAsBytes()),
                        Paused = !IsAutoStart,
                        DownloadDirectory = DownlaodDir ?? settings?.DownloadDirectory,
                        PeerLimit = PeerLimit,
                        FilesUnwanted = FilesTree?.getFilesWantedStatus(false),
                        FilesWanted = FilesTree?.getFilesWantedStatus(true)
                    };

                    Log.Info($"Start adding torrentFileInfo {newTorrent.Filename}");

                    while (true)
                    {
                        if (Dispatcher.CurrentDispatcher.HasShutdownStarted) return;
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
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


                                            await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                                            {
                                                IDs = new[] {TorrentResult.Value.ID},
                                                TrackerAdd = toAdd
                                            }, cancellationToken);

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
                                Log.Info(
                                    $"Adding torrentFileInfo result '{TorrentResult.Value.Status}' '{newTorrent.Filename}'");
                                await Task.Delay(100, cancellationToken);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                            return;
                        }
                    }
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
            _filesTree = null;
            cancellationTokenSource?.Cancel();
            settings = null;
            _torrentFileInfo = null;
            _torrent = null;
        }
    }
}