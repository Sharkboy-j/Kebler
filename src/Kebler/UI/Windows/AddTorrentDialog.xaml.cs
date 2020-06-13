using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Kebler.Services;
using log4net;
using Microsoft.Win32;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;
using Transmission.API.RPC.Response;

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for AddTorrentDialog.xaml
    /// </summary>
    public partial class AddTorrentDialog : CustomWindow, INotifyPropertyChanged
    {

        public AddTorrentDialog(string path, SessionSettings settings, ref TransmissionClient transmissionClient)
        {
            torrent = new FileInfo(path);
            TorrentPath = torrent.FullName;
            DownlaodDir = settings.DownloadDirectory;
            _transmissionClient = transmissionClient;
            this.settings = settings;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            IsAutoStart = true;
            if (!ConfigService.Instanse.IsAddTorrentWindowShow)
            {
                InitializeComponent();
                DataContext = this;
                IsWorking = false;
                IsAddTorrentWindowShow = ConfigService.Instanse.IsAddTorrentWindowShow;
            }
            Result.Visibility = Visibility.Collapsed;
            PeerLimit = ConfigService.Instanse.TorrentPeerLimit;
        }

        private void ChangePath(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = false,
                InitialDirectory = torrent.DirectoryName
            };

            if (openFileDialog.ShowDialog() != true) return;

            torrent = new FileInfo(openFileDialog.FileName);
            TorrentPath = torrent.FullName;
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

        public void Add(object sender, RoutedEventArgs e)
        {
            Result.Visibility = Visibility.Collapsed;
            IsWorking = true;
            Task.Factory.StartNew(async () =>
            {
                if (!File.Exists(TorrentPath))
                    throw new Exception("Torrent file not found");


                var filebytes = await File.ReadAllBytesAsync(TorrentPath, cancellationToken);

                var encodedData = Convert.ToBase64String(filebytes);
                //string decodedString = Encoding.UTF8.GetString(filebytes);
                var torrent = new NewTorrent
                {
                    Metainfo = encodedData,
                    Paused = !IsAutoStart,
                    DownloadDirectory = DownlaodDir ?? settings.DownloadDirectory,
                    PeerLimit = PeerLimit
                };


                //Debug.WriteLine($"Start Adding torrent {path}");

                Log.Info($"Start adding torrent {torrent.Filename}");

                while (true)
                {
                    if (Dispatcher.HasShutdownStarted) return;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        TorrentResult = await _transmissionClient.TorrentAddAsync(torrent);

                        if (TorrentResult.Result == Enums.ReponseResult.Ok)
                        {
                            switch (TorrentResult.Value.Status)
                            {
                                case Enums.AddTorrentStatus.Added:
                                    {
                                        Log.Info($"Torrent {torrent.Filename} added as {TorrentResult.Value.Name}");
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
                                        Dispatcher.Invoke(() =>
                                        {
                                            Result.Content = Kebler.Resources.Dialogs.ATD_TorrentExist;
                                            Result.Visibility = Visibility.Visible;
                                        });
                                        return;
                                    }
                            }
                        }
                        else
                        {
                            Log.Info($"Adding torrent result '{TorrentResult.Value.Status}' {torrent.Filename}");
                            await Task.Delay(100);
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
                    finally
                    {
                        IsWorking = false;
                    }
                }
            }, cancellationToken);
        }

        private void ChangeVisibilityWindow(object sender, RoutedEventArgs e)
        {
            ConfigService.Instanse.IsAddTorrentWindowShow = IsAddTorrentWindowShow;
            ConfigService.Save();
        }


    }

    public partial class AddTorrentDialog
    {
        FileInfo torrent;
        SessionSettings settings;
        TransmissionClient _transmissionClient;
        public AddTorrentResponse TorrentResult;


        public event PropertyChangedEventHandler PropertyChanged;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        CancellationTokenSource cancellationTokenSource;
        CancellationToken cancellationToken;


        public string TorrentPath { get; set; }
        public string DownlaodDir { get; set; }
        public bool IsWorking { get; set; }
        public bool IsAddTorrentWindowShow { get; set; }
        public int PeerLimit { get; set; }
        public int UploadLimit { get; set; }
        public bool IsAutoStart { get; set; }
    }
}
