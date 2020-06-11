using Kebler.Services;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;

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
                this.DataContext = this;
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
                    PeerLimit = this.PeerLimit,
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

                        switch (TorrentResult.Result)
                        {
                            case Enums.AddResult.Duplicate:
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        Result.Content = Kebler.Resources.Dialogs.ATD_TorrentExist;
                                        Result.Visibility = Visibility.Visible;
                                    });
                                    return;
                                }
                            case Enums.AddResult.Added:
                                {
                                    Log.Info($"Torrent {torrent.Filename} added as {TorrentResult.Name}");
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
                            case Enums.AddResult.Error:
                            case Enums.AddResult.ResponseNull:
                                {
                                    Log.Info($"Adding torrent result Null {torrent.Filename}");
                                    await Task.Delay(100);
                                    continue;
                                }
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
        public TorrentAddResult TorrentResult;


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
