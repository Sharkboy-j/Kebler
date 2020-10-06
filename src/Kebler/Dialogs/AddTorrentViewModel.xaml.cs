using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Response;
using Kebler.Resources;
using Kebler.Services;
using Kebler.TransmissionCore;
using Kebler.ViewModels;
using log4net;
using Microsoft.Win32;
// ReSharper disable MemberCanBePrivate.Global

namespace Kebler.Dialogs
{
    /// <summary>
    ///     Interaction logic for AddTorrentView.xaml
    /// </summary>
    public partial class AddTorrentView
    {

        private TorrentInfo? torrentInfo;
        public AddTorrentView(string path, TransmissionClient transmissionClient, Window owner,
            SessionSettings settings) : base(owner)
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
                InitializeComponent();
                DataContext = this;
                IsWorking = false;
                IsAddTorrentWindowShow = ConfigService.Instanse.IsAddTorrentWindowShow;
            }

            Result.Visibility = Visibility.Collapsed;
            PeerLimit = ConfigService.Instanse.TorrentPeerLimit;

            Loaded += AddTorrentDialog_Loaded;
            FilesTree.Border = new Thickness(1);
        }

        private async void AddTorrentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTree();
            LoadingSettingsGrid.Visibility = Visibility.Collapsed;
        }

        private void ChangePath(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Torrent files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = false,
                InitialDirectory = _torrentFileInfo.DirectoryName ?? throw new InvalidOperationException()
            };

            if (openFileDialog.ShowDialog() != true) return;

            _torrentFileInfo = new FileInfo(openFileDialog.FileName);
            TorrentPath = _torrentFileInfo.FullName;
        }


        private void Cancel(object sender, RoutedEventArgs e)
        {
            IsWorking = false;
            DialogResult = false;
            cancellationTokenSource.Cancel();
            Close();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            IsWorking = false;
            cancellationTokenSource.Cancel();
        }

        private async void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            await LoadTree();

            LoadingSettingsGrid.Visibility = Visibility.Collapsed;
        }

        private async Task LoadTree()
        {
            LoadingSettingsGrid.Visibility = Visibility.Visible;

            try
            {
                data = await File.ReadAllBytesAsync(TorrentPath, cancellationToken);
                TorrentInfo.TryParse(data, out torrentInfo);
                FilesTree.UpdateFilesTree(torrentInfo);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        public void Add(object sender, RoutedEventArgs e)
        {
            Result.Visibility = Visibility.Collapsed;
            IsWorking = true;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    //var dd = FilesTree.getFilesWantedStatus(false);

                    var encodedData = Convert.ToBase64String(data);
                    //string decodedString = Encoding.UTF8.GetString(filebytes);
                    var newTorrent = new NewTorrent
                    {
                        Metainfo = encodedData,
                        Paused = !IsAutoStart,
                        DownloadDirectory = DownlaodDir ?? settings.DownloadDirectory,
                        PeerLimit = PeerLimit,
                        FilesUnwanted = FilesTree.getFilesWantedStatus(false),
                        FilesWanted = FilesTree.getFilesWantedStatus(true)
                    };


                    //Debug.WriteLine($"Start Adding torrentFileInfo {path}");

                    Log.Info($"Start adding torrentFileInfo {newTorrent.Filename}");

                    while (true)
                    {
                        if (Dispatcher.HasShutdownStarted) return;
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            TorrentResult = await _transmissionClient.TorrentAddAsync(newTorrent, cancellationToken);

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
                                        Dispatcher.Invoke(() =>
                                        {
                                            DialogResult = true;
                                            Close();
                                        });
                                        return;
                                    }
                                    case Enums.AddTorrentStatus.Duplicate:
                                    {
                                        Log.Info(
                                            $"Adding torrentFileInfo result '{TorrentResult.Value.Status}' '{TorrentResult.Value.Name}'");

                                        var getExistSettings =
                                            await _transmissionClient.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS, cancellationToken,
                                                new[] {TorrentResult.Value.ID});


                                        if (getExistSettings.Torrents.Length == 1)
                                        {
                                            var torrent = getExistSettings.Torrents.First();

                                            var toAdd = new List<string>();
                                            foreach (var tracker in torrentInfo.Trackers)
                                            {
                                                if (!torrent.Trackers.Contains(tracker))
                                                {
                                                    toAdd.Add(tracker.announce);
                                                }
                                            }
                                            
                                            await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                                            {
                                                IDs =  new []{TorrentResult.Value.ID},
                                                TrackerAdd = toAdd.ToArray()
                                            }, cancellationToken);
                                            foreach (var tr in toAdd)
                                            {
                                                Log.Info($"Tracker added: {tr}'");
                                            }
                                        }
                                      
                                        
                                       
                                        
                                        Dispatcher.Invoke(() =>
                                        {
                                            Result.Content = Strings.ATD_TorrentExist;
                                            Result.Visibility = Visibility.Visible;
                                        });
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

        private void ChangeVisibilityWindow(object sender, RoutedEventArgs e)
        {
            ConfigService.Instanse.IsAddTorrentWindowShow = IsAddTorrentWindowShow;
            ConfigService.Save();
        }


        private void AddTorrentDialog_OnClosing(object sender, CancelEventArgs e)
        {
            FilesTree = null;
            cancellationTokenSource?.Cancel();
            data = null;
            settings = null;
            _torrentFileInfo = null;
            Loaded -= AddTorrentDialog_Loaded;
            DataContext = null;
            torrentInfo = null;
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
    }

    public partial class AddTorrentView
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly TransmissionClient _transmissionClient;
        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        private FileInfo? _torrentFileInfo;
        private byte[]? data;
        private SessionSettings? settings;
        public AddTorrentResponse TorrentResult;


        public string TorrentPath { get; set; }
        public string DownlaodDir { get; set; }
        public bool IsWorking { get; set; }
        public bool IsAddTorrentWindowShow { get; set; }
        public int PeerLimit { get; set; }
        public int UploadLimit { get; set; }
        public bool IsAutoStart { get; set; }

        public FilesTreeViewModel? FilesTree { get; private set; } = new FilesTreeViewModel();
    }
}