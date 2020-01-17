using Kebler.Models;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.UI.Windows;
using Kebler.UI.Windows.Dialogs;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;
using Enums = Transmission.API.RPC.Entity.Enums;

namespace Kebler.UI.ViewModels
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<Server> _serversList;
        private LiteCollection<Server> _dbServers;
        private Statistic _stats;
        private SessionInfo _sessionInfo;
        private TransmissionClient _transmissionClient;
        private ConnectionManager _cmWindow;
        private Task _whileCycleTask;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private Category.Categories _filterCategory = Category.Categories.All;


        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public Server ConnectedServer { get; set; }
        public bool IsSlowModeEnabled { get; set; }

        public bool IsDoingStuff { get; set; }
        // ReSharper disable once UnusedMember.Global
        public bool IsDoingStuffReverse => !IsDoingStuff;

        public string LongStatusText { get; set; }

        public ObservableCollection<TorrentInfo> TorrentList { get; set; } = new ObservableCollection<TorrentInfo>();
        private TorrentInfo[] allTorrents = new TorrentInfo[0];
        private SessionSettings _settings;
        public TorrentInfo SelectedTorrent { get; set; }

        public bool IsErrorOccuredWhileConnecting { get; set; }
        public int TorrentDataGridSelectedIndex { get; set; } = 0;


        #region StatusBarProps
        public string IsConnectedStatusText { get; set; } = string.Empty;
        public string DownloadSpeed { get; set; } = string.Empty;
        public string UploadSpeed { get; set; } = string.Empty;


        #endregion

        public MainWindowViewModel()
        {
            Task.Factory.StartNew(InitConnection);
        }


        public void InitConnection()
        {
            if (IsConnected) return;

            GetAllServers();

            if (_serversList.Count == 0)
            {
                ShowConnectionManager();
            }
            else
            {
                new Task(() => { TryConnect(_serversList.FirstOrDefault()); }).Start();
            }
        }

        public void AddTorrent(string path)
        {
            new Task(async () =>
            {
                if (!File.Exists(path))
                    throw new Exception("Torrent file not found");

                await using var fstream = File.OpenRead(path);

                var filebytes = new byte[fstream.Length];
                fstream.Read(filebytes, 0, Convert.ToInt32(fstream.Length));

                var encodedData = Convert.ToBase64String(filebytes);
                string decodedString = Encoding.UTF8.GetString(filebytes);
                var torrent = new NewTorrent
                {
                    Metainfo = encodedData,
                    Paused = false
                };

              

                //Debug.WriteLine($"Start Adding torrent {path}");
                Log.Info($"Start adding torrent {path}");

                //try upload torrent
                while (true)
                {
                    if (!IsConnected)
                    {
                        await Task.Delay(500);
                        continue;
                    }
                    if (Dispatcher.HasShutdownStarted) return;
                    try
                    {
                        var info = await _transmissionClient.TorrentAddAsync(torrent);

                        switch (info)
                        {
                            case Enums.AddResult.Error:
                            case Enums.AddResult.Duplicate:
                            case Enums.AddResult.Added:
                                {
                                    Debug.WriteLine($"Torrent {path} added as {info}");
                                    Log.Info($"Torrent {path} added as {info}");
                                    return;
                                }
                            case Enums.AddResult.ResponseNull:
                                {
                                    //Debug.WriteLine($"Adding torrent result Null {torrent.Filename}");
                                    Log.Info($"Adding torrent result Null {torrent.Filename}");
                                    await Task.Delay(100);
                                    continue;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        return;
                    }
                }
            }).Start();
        }



        public void ShowConnectionManager()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _cmWindow = new ConnectionManager(ref _dbServers);
                _cmWindow.ShowDialog();
            });
        }
        private async void TryConnect(Server server)
        {
            if (server == null) return;

            Log.Info("Start Connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = "Connecting";
            try
            {
                IsConnecting = true;
                var pass = string.Empty;
                if (server.AskForPassword)
                {
                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass)) return;

                }

                try
                {
                    _transmissionClient = new TransmissionClient(server.FullUriPath, null, server.UserName, server.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(server.Password));

                    _sessionInfo = await UpdateSessionInfo();
                    IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})";
                    ConnectedServer = server;
                    StartWhileCycle();
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                    IsConnectedStatusText = ex.Message;
                    IsConnected = false;
                    IsErrorOccuredWhileConnecting = true;
                }
            }
            finally
            {
                IsConnecting = false;
            }



        }
        private static async Task<string> GetPassword()
        {
            var pass = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new DialogPassword();
                pass = dialog.ShowDialog() == true ? dialog.ResponseText : string.Empty;
            });
            return pass;
        }


        private void GetAllServers()
        {
            _dbServers = StorageRepository.GetServersList();
            _serversList = new List<Server>(_dbServers.FindAll() ?? new List<Server>());
            _serversList.LogServers();
        }

        private void StartWhileCycle()
        {

            if (_whileCycleTask != null)
                _cancelTokenSource.Cancel();
            _whileCycleTask?.Dispose();

            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;



            _whileCycleTask = new Task(async () =>
            {
                try
                {
                    while (IsConnected && !token.IsCancellationRequested)
                    {

                        if (Application.Current.Dispatcher.HasShutdownStarted) return;

                        Debug.WriteLine("Start updating stats");
                        _stats = await UpdateStats();
                        Debug.WriteLine("Stats updated");

                        //sessionInfo = await UpdateSessionInfo();
                        _settings = await _transmissionClient.GetSessionSettingsAsync();
                        IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;
                        Debug.WriteLine("Start updating torrents");
                        var newTorrentsDataList = await UpdateTorrentList();
                        Debug.WriteLine("Torrents updated");

                        allTorrents = newTorrentsDataList.Torrents;
                        //List<TorrentInfo> torrents;

                        //var prop = typeof(TorrentInfo).GetProperty(ConfigService.ConfigurationData.SortVal);

                        //if (prop != null)
                        //{
                        //    switch (ConfigService.ConfigurationData.SortType)
                        //    {
                        //        case 0:
                        //            torrents = newTorrentsDataList?.Torrents.OrderBy(x => prop.GetValue(x, null)).ToList();
                        //            break;
                        //        case 1:
                        //            torrents = newTorrentsDataList?.Torrents.OrderByDescending(x => prop.GetValue(x, null)).ToList();
                        //            break;
                        //        default:
                        //            torrents = newTorrentsDataList?.Torrents.ToList();
                        //            break;
                        //    }
                        //}
                        //else
                        //{
                        //    torrents = newTorrentsDataList?.Torrents.ToList();
                        //}

                        //UpdateTorrentListAtUi(ref torrents);
                        UpdateSorting(newTorrentsDataList);
                        UpdateStatsInfo();
                        await Task.Delay(1000, token);
                    }
                }
                catch (TaskCanceledException)
                {
                }
            }, token);
            _whileCycleTask.Start();
        }

        public void UpdateSorting(TransmissionTorrents newTorrents = null)
        {
            var prop = typeof(TorrentInfo).GetProperty(ConfigService.ConfigurationData.SortVal);

            if (prop == null) return;
            var torrentsToSort = newTorrents == null ? allTorrents.ToList() : newTorrents.Torrents.ToList();

            //  var torrents = new List<TorrentInfo>(TorrentList);
            //1: 'check pending',
            //2: 'checking',

            //5: 'seed pending',
            //6: 'seeding',
            switch (_filterCategory)
            {
                //3: 'download pending',
                //4: 'downloading',
                case Category.Categories.Downloading:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 3 || x.Status == 4).ToList();
                    break;

                //4: 'downloading',
                //6: 'seeding',
                //2: 'checking',
                case Category.Categories.Active:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToList();
                    torrentsToSort = torrentsToSort.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToList();
                    break;

                //0: 'stopped' and is error,
                case Category.Categories.Stopped:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString)).ToList();
                    break;

                case Category.Categories.Error:
                    torrentsToSort = torrentsToSort.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToList();
                    break;


                //4: 'downloading',
                //6: 'seeding',
                //2: 'checking',
                case Category.Categories.Inactive:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToList();
                    torrentsToSort = torrentsToSort.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0).ToList();
                    break;

                //6: 'seeding',
                case Category.Categories.Ended:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 6).ToList();
                    break;
                case Category.Categories.All:
                default:
                    break;
            }


            torrentsToSort = ConfigService.ConfigurationData.SortType switch
            {
                0 => torrentsToSort.OrderBy(x => prop.GetValue(x, null)).ToList(),
                1 => torrentsToSort.OrderByDescending(x => prop.GetValue(x, null)).ToList(),
                _ => torrentsToSort.ToList(),
            };

            UpdateTorrentListAtUi(ref torrentsToSort);



        }

        #region ServerActions
        private async Task<TransmissionTorrents> UpdateTorrentList()
        {
            return await ExecuteLongTask(_transmissionClient.TorrentGetAsync, "Getting torrents");
        }
        private async Task<Statistic> UpdateStats()
        {
            return await ExecuteLongTask(_transmissionClient.GetSessionStatisticAsync, "Updating stats");
        }


        private async Task<SessionInfo> UpdateSessionInfo()
        {
            return await ExecuteLongTask(_transmissionClient.GetSessionInformationAsync, "Connecting to session");
        }

        public void ChangeFilterType(Category.Categories cat)
        {
            _filterCategory = cat;
            UpdateSorting();
        }



        public async void RemoveTorrent(bool removeData = false)
        {
            if (!IsConnected) return;

            var dialog = new DialogBox("You are perform to remove %n torrents with data", "Please, confirm action.");
            dialog.ShowDialog();
            if (dialog.Response)
            {

                var torrentInfo = SelectedTorrent;

                await Task.Factory.StartNew(async () =>
                {
                    Log.Info($"Try remove {torrentInfo.Name}");

                    var exitCondition = Enums.RemoveResult.Error;
                    while (exitCondition != Enums.RemoveResult.Ok)
                    {
                        if (Dispatcher.HasShutdownStarted) return;

                        exitCondition = await _transmissionClient.TorrentRemoveAsync(new[] { torrentInfo.ID }, removeData);

                        await Task.Delay(500);
                    }
                    Log.Info($"Removed {torrentInfo.Name}");
                });
            }

        }
        public void SlowMode()
        {
            _transmissionClient.SetSessionSettingsAsync(new SessionSettings() { AlternativeSpeedEnabled = !_settings.AlternativeSpeedEnabled });
        }

        public async void PauseTorrent()
        {
            if (!IsConnected) return;

            var torrentInfo = SelectedTorrent;

            await Task.Factory.StartNew(async () =>
            {
                Log.Info($"Try pause {torrentInfo.Name}");

                await _transmissionClient.TorrentStopAsync(new[] { torrentInfo.ID });

                Log.Info($"Paused {torrentInfo.Name}");
            });


        }

        public async void StartOne()
        {
            if (!IsConnected) return;

            var torrentInfo = SelectedTorrent;

            await Task.Factory.StartNew(() =>
            {
                Log.Info($"Try start {torrentInfo.Name}");

                _transmissionClient.TorrentStartAsync(new[] { torrentInfo.ID });

                Log.Info($"Started {torrentInfo.Name}");
            });
        }

        public async void StartAll()
        {
            if (!IsConnected) return;

            var torrentIds = TorrentList.Select(x => x.ID).ToArray();
            await Task.Factory.StartNew(() =>
            {
                Log.Info($"Try start all torrents");

                _transmissionClient.TorrentStartAsync(torrentIds);

                Log.Info($"Started all");
            });
        }

        public async void StopAll()
        {
            if (!IsConnected) return;

            var torrentIds = TorrentList.Select(x => x.ID).ToArray();
            await Task.Factory.StartNew(async () =>
            {
                Log.Info($"Try pause all torrents");

                await _transmissionClient.TorrentStopAsync(torrentIds);

                Log.Info($"Paused all");
            });
        }

        private async Task<dynamic> ExecuteLongTask(Func<dynamic> asyncTask, string longStatusText)
        {
            while (true)
            {
                Debug.WriteLine($"Start getting {nameof(asyncTask)}");
                if (Dispatcher.HasShutdownStarted) return null;
                var resp = await asyncTask();
                if (resp == null)
                {
                    Debug.WriteLine($"w8ing for response");
                    IsDoingStuff = true;
                    LongStatusText = longStatusText;
                    await Task.Delay(100);
                }
                else
                {
                    IsDoingStuff = false;
                    LongStatusText = string.Empty;
                    Debug.WriteLine($"Response received");
                    return resp;
                }
            }
        }
        #endregion


        #region ParsersToUserFriendlyFormat
        private void UpdateTorrentListAtUi(ref List<TorrentInfo> list)
        {

            if (list == null) return;
            //TorrentList =  new ObservableCollection<TorrentInfo>(list);
            //add new one or update old
            for (var ind = 0; ind < list.Count; ind++)
            {
                var item = list[ind];

                if (TorrentList.Any(x => x.ID == item.ID))
                {
                    var data = TorrentList.First(x => x.ID == item.ID);

                    UpdateTorrentData(TorrentList.IndexOf(data), ind, item);
                }
                else
                {
                    Dispatcher.Invoke(() => { TorrentList.Add(item); });
                }
            }


            //remove removed torrent
            foreach (var item in TorrentList.ToList())
            {
                if (list.All(x => x.ID != item.ID))
                {
                    var data = TorrentList.First(x => x.ID == item.ID);
                    var ind = TorrentList.IndexOf(data);
                    if (SelectedTorrent != null && SelectedTorrent.ID == data.ID) SelectedTorrent = null;
                    Dispatcher.Invoke(() => { TorrentList.RemoveAt(ind); });

                }
            }


        }

        private void UpdateTorrentData(int index, int newIndex, TorrentInfo newData)
        {
            if (SelectedTorrent == null) return;
            if (SelectedTorrent.ID == newData.ID) SelectedTorrent = newData;

            var myType = typeof(TorrentInfo);
            var props = myType.GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);
            foreach (var item in props)
            {
                TorrentList[index].Set(item.Name, newData.Get(item.Name));
            }
            if (index != newIndex)
                Dispatcher.Invoke(() =>
                {
                    TorrentList.Move(index, newIndex);
                });

            //TorrentList[index].RateUpload = newData.RateUpload;
            //TorrentList[index].RateDownload = newData.RateDownload;
            //TorrentList[index].AddedDate = newData.AddedDate;
            //TorrentList[index].Pieces = newData.Pieces;
            //TorrentList[index].PieceCount = newData.PieceCount;
            //TorrentList[index].DownloadedEver = newData.DownloadedEver;
            //TorrentList[index].PercentDone = newData.PercentDone;

        }

        private void UpdateStatsInfo()
        {
            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.downloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.uploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;

            DownloadSpeed = $"D: {dSpeed}";
            UploadSpeed = $"U: {uSpeed}";
        }
        #endregion





        #region Events

        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            new Task(() => { TryConnect(_serversList.FirstOrDefault()); }).Start();

        }
        #endregion


    }
}
