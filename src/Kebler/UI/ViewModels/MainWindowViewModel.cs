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
using System.Runtime.CompilerServices;

namespace Kebler.UI.ViewModels
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<Server> _serversList;
        private LiteCollection<Server> _dbServers;
        private Statistic _stats;
        private SessionInfo _sessionInfo;
        public TransmissionClient _transmissionClient;
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
        public string FilterText { get; set; }

        public string Title
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion}";
            }
        }

        public ObservableCollection<TorrentInfo> TorrentList { get; set; } = new ObservableCollection<TorrentInfo>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private TorrentInfo[] allTorrents = new TorrentInfo[0];
        public SessionSettings _settings;
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
            App.Instance.LangChanged += Instance_LangChanged;

            Task.Factory.StartNew(InitConnection);
        }

        private void Instance_LangChanged()
        {
            IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                               $"      {Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.uploadedBytes)}" +
                               $"      {Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                               $"      {Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";
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
                    Log.Info("Manual ask password");

                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass)) return;

                }

                try
                {
                    _transmissionClient = new TransmissionClient(server.FullUriPath, null, server.UserName, server.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(server.Password));
                    Log.Info("Client created");

                    _sessionInfo = await UpdateSessionInfo();

                    Log.Info($"Connected {_sessionInfo.Version}");

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
            Log.Info("StartWhileCycle");

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

                        if (Application.Current?.Dispatcher != null && Application.Current.Dispatcher.HasShutdownStarted) return;

                        _stats = await UpdateStats();
                        Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;
                        IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                $"      {Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.uploadedBytes)}" +
                                $"      {Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                $"      {Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                        //sessionInfo = await UpdateSessionInfo();
                        _settings = await _transmissionClient.GetSessionSettingsAsync();

                        if (_settings == null) continue;

                        if (_settings.AlternativeSpeedEnabled != null)
                            IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;
                        var newTorrentsDataList = await UpdateTorrentList();

                        allTorrents = newTorrentsDataList.Torrents;


                        //UpdateTorrentListAtUi(ref torrents);
                        UpdateCategories(newTorrentsDataList.Torrents.Select(x => x.DownloadDir).ToList());
                        UpdateSorting(newTorrentsDataList);
                        UpdateStatsInfo();
                        await Task.Delay(ConfigService.Instanse.UpdateTime, token);
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }, token);
            _whileCycleTask.Start();
        }


        //TODO: add auto removing
        private void UpdateCategories(IEnumerable<string> dirrectories)
        {
            if (dirrectories == null) return;

            var dirs = dirrectories.Distinct().ToList();


            foreach (var path in dirs)
            {
                var cat = new Category(path);
                cat.Title += $"{cat.FolderName} ({allTorrents.Count(x => x.DownloadDir == cat.FullPath)})";
                if (!IsExist(Categories, cat))
                    Dispatcher.Invoke(() =>
                    {
                        Categories.Add(cat);
                    });
            }
        }

        public bool IsExist(IEnumerable<Category> list, Category item)
        {
            return list.Any(x => x.FolderName == item.FolderName);
        }

        public void UpdateSorting(TransmissionTorrents newTorrents = null)
        {
            var prop = typeof(TorrentInfo).GetProperty(ConfigService.Instanse.SortVal);

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


            torrentsToSort = ConfigService.Instanse.SortType switch
            {
                0 => torrentsToSort.OrderBy(x => prop.GetValue(x, null)).ToList(),
                1 => torrentsToSort.OrderByDescending(x => prop.GetValue(x, null)).ToList(),
                _ => torrentsToSort.ToList(),
            };
            FilterTorrents(ref torrentsToSort);
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


        public async void SetLocation()
        {
            if (!IsConnected) return;

            var torrentInfo = SelectedTorrent;

            var dialog = new DialogTextBox(torrentInfo.DownloadDir, "Please, enter new location.");

            if ((bool)dialog.ShowDialog())
            {
                await Task.Factory.StartNew(async () =>
                {
                    Log.Info($"Try set location {torrentInfo.Name}({torrentInfo.DownloadDir} => {dialog.Response})");

                    var exitCondition = Enums.RemoveResult.Error;
                    while (exitCondition != Enums.RemoveResult.Ok)
                    {
                        if (Dispatcher.HasShutdownStarted) return;
                        _transmissionClient.TorrentSetLocationAsync(new[] { torrentInfo.ID }, dialog.Response, true);

                        await Task.Delay(500);
                    }
                    Log.Info($"Location set {torrentInfo.Name} true");
                });

            }




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
               // Debug.WriteLine($"Start getting {nameof(asyncTask)}");
                if (Dispatcher.HasShutdownStarted)
                    return null;

                var resp = await asyncTask();
                if (resp == null)
                {
                   // Debug.WriteLine($"w8ing for response {nameof(asyncTask)}");
                    IsDoingStuff = true;
                    LongStatusText = longStatusText;
                    await Task.Delay(100);
                }
                else
                {
                    IsDoingStuff = false;
                    LongStatusText = string.Empty;
                   // Debug.WriteLine($"Response received {nameof(asyncTask)}");
                    return resp;
                }
            }
        }
        #endregion


        #region ParsersToUserFriendlyFormat
        private void UpdateTorrentListAtUi(ref List<TorrentInfo> list)
        {
            lock (TorrentList)
            {
                if (list == null) return;

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




            }
        }
        private void FilterTorrents(ref List<TorrentInfo> list)
        {
            if (string.IsNullOrEmpty(FilterText))
                return;

            var filterType = FilterText.Contains("{p}:") ? Models.Enums.FilterType.Path : Models.Enums.FilterType.Name;
            var splited = FilterText.Split("{p}:");
            var filterKey = filterType == Models.Enums.FilterType.Path ? splited[^1] : FilterText;


            switch (filterType)
            {
                case Models.Enums.FilterType.Name:
                    list = list.Where(x => x.Name.ToLower().Contains(filterKey.ToLower())).ToList();
                    break;
                case Models.Enums.FilterType.Path:
                    list = list.Where(x => x.DownloadDir.Equals(filterKey)).ToList();
                    break;
            }
        }


        private void UpdateTorrentData(int index, int newIndex, TorrentInfo newData)
        {
            if (SelectedTorrent == null) return;
            if (SelectedTorrent.ID == newData.ID) SelectedTorrent = newData;

            var myType = typeof(TorrentInfo);
            var props = myType.GetProperties().Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);

            foreach (var item in props)
            {
                TorrentList[index].Set(item.Name, newData.Get(item.Name));
            }
            if (index != newIndex)
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        TorrentList.Move(index, newIndex);
                    }
                    catch
                    {
                        // ignored
                    }
                });

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

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
