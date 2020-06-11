using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Transmission.API.RPC.Entity;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using Kebler.Models;
using Microsoft.Win32;
using Kebler.Services;
using System.Windows.Input;
using Kebler.UI.Controls;
using Kebler.Services.Converters;
using Kebler.UI.Windows;
using LiteDB;
using log4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Enums = Transmission.API.RPC.Entity.Enums;
using System.Runtime.CompilerServices;
using MessageBox = Kebler.UI.Windows.MessageBox;
using Kebler.UI.Dialogs;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow : INotifyPropertyChanged
    {

        public KeblerWindow()
        {
           

            InitializeComponent();

            ApplyConfig();

            DataContext = this;

            App.Instance.LangChanged += Instance_LangChanged;

            Task.Factory.StartNew(InitConnection);

            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_AllTorrents, Tag = Kebler.Models.Enums.Categories.All });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_Downloading, Tag = Kebler.Models.Enums.Categories.Downloading });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_Active, Tag = Kebler.Models.Enums.Categories.Active });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_InActive, Tag = Kebler.Models.Enums.Categories.Inactive });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_Ended, Tag = Kebler.Models.Enums.Categories.Ended });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_Stopped, Tag = Kebler.Models.Enums.Categories.Stopped });
            CategoriesListBox.Items.Add(new ListBoxItem() { Name = Kebler.Resources.Strings.Cat_Error, Tag = Kebler.Models.Enums.Categories.Error });

            CategoriesListBox.SelectedIndex = 0;
            Init_HK();


        }

        private void Init_HK()
        {
            new HotKey(Key.C, KeyModifier.Shift | KeyModifier.Ctrl, ShowConnectionManager);
            new HotKey(Key.N, KeyModifier.Ctrl, AddTorrent);
        }

        private void ApplyConfig()
        {
            try
            {
                this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                Width = ConfigService.Instanse.MainWindowWidth;
                Height = ConfigService.Instanse.MainWindowHeight;
                WindowState = ConfigService.Instanse.MainWindowState;
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                if (!string.IsNullOrEmpty(ConfigService.Instanse.ColumnSizes))
                {
                    var splited = ConfigService.Instanse.ColumnSizes.Split(',');
                    for (int i = 1; i < splited.Length; i++)
                    {
                        TorrentsDataGrid.Columns[i].Width = Convert.ToDouble(splited[i]);
                    }
                }
               

            }
            catch (Exception ex)
            {
                Log.Error(ex);

                var msg = new MessageBox(Kebler.Resources.Dialogs.ConfigApllyError, Kebler.Resources.Dialogs.Error, Models.Enums.MessageBoxDilogButtons.Ok, true);
                msg.ShowDialog();
            }


        }

        public void Connect()
        {
            InitConnection();
        }

        #region ToolBar actions
        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(string[] names)
        {
            foreach (var item in names)
            {
                var dialog = new AddTorrentDialog(item, _settings, ref _transmissionClient);
                dialog.Owner = this;

                if (!(bool)dialog.ShowDialog())
                    return;

                if (dialog.TorrentResult.Result == Enums.AddResult.Added)
                {
                    TorrentList.Add(new TorrentInfo()
                    {
                        ID = dialog.TorrentResult.ID,
                        Name = dialog.TorrentResult.Name,
                        HashString = dialog.TorrentResult.HashString
                    });

                    OnPropertyChanged(nameof(TorrentList));
                }

                dialog = null;
            }
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            IsErrorOccuredWhileConnecting = false;
            InitConnection();
        }

        private void TorrentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectedTorrents = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToArray();
        }



        private void PauseTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            Task.Factory.StartNew(() =>
            {
                return _transmissionClient.TorrentStopAsync(SelectedTorrents.Select(x => x.ID).ToArray());

            });
        }

        private async void StartAll_Button_CLick(object sender, RoutedEventArgs e)
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

        private void AddTorrentButtonClick(object sender, RoutedEventArgs e)
        {
            AddTorrent();
        }

        private async void PauseAll_ButtonClick(object sender, RoutedEventArgs e)
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

        private void StartSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            Task.Factory.StartNew(() =>
            {
                _transmissionClient.TorrentStartAsync(SelectedTorrents.Select(x => x.ID).ToArray());
            });
        }

        private void RemoveTorrent_ButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent();
        }
        private void RemoveTorrentWithData_ButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent(true);
        }


        public async void RemoveTorrent(bool removeData = false)
        {

            var obj = new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), removeData);
            var dialog = new MessageBox(obj, true, string.Empty, Models.Enums.MessageBoxDilogButtons.OkCancel, true);
            dialog.Owner = this;


            if ((bool)dialog.ShowDialog())
            {
                if (!IsConnected)
                    new MessageBox(Kebler.Resources.Dialogs.ConnectionLost, Kebler.Resources.Dialogs.Error, Models.Enums.MessageBoxDilogButtons.Ok, true).Show();


                await Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (Dispatcher.HasShutdownStarted)
                            return;

                        var exitCondition = await _transmissionClient.TorrentRemoveAsync(SelectedTorrents.Select(x => x.ID).ToArray(), obj.WithData);

                        if (exitCondition == Enums.RemoveResult.Ok)
                            break;
                        else
                            await Task.Delay(500);
                    }
                });
            }

            obj = null;
            dialog = null;
        }

        private void SlowMode_Click(object sender, RoutedEventArgs e)
        {
            IsSlowModeEnabled = !IsSlowModeEnabled;
            _transmissionClient.SetSessionSettingsAsync(new SessionSettings() { AlternativeSpeedEnabled = !_settings.AlternativeSpeedEnabled });
        }

        #endregion


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            if (!(sender is System.Windows.Controls.ListBox listBox)) return;

            _filterCategory = (Kebler.Models.Enums.Categories)((ListBoxItem)listBox.SelectedValue).Tag;

            UpdateSorting();
        }


        private async void SetLocation_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;


            var dialog = new MessageBox(true, "Please, enter new location", false);
            dialog.Owner = Application.Current.MainWindow;
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    await Task.Factory.StartNew(async () =>
                    {

                        var exitCondition = Enums.RemoveResult.Error;
                        while (exitCondition != Enums.RemoveResult.Ok)
                        {
                            if (Dispatcher.HasShutdownStarted) return;
                            _transmissionClient.TorrentSetLocationAsync(SelectedTorrents.Select(x => x.ID).ToArray(), dialog.Value, true);
                            await Task.Delay(500);
                        }
                    });
                }
            }
            finally
            {

                dialog.Value = null;
                dialog = null;
            }
        }

        private void Vertical_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ConfigService.Instanse.CategoriesWidth = CategoriesColumn.ActualWidth;
            ConfigService.Save();
        }

        private void ClosingW(object sender, CancelEventArgs e)
        {
            var size = new StringBuilder();
            foreach (DataGridColumn column in TorrentsDataGrid.Columns)
            {
                size.Append($"{column.ActualWidth},");
            }
            

            ConfigService.Instanse.MainWindowHeight = this.ActualHeight;
            ConfigService.Instanse.MainWindowWidth = this.ActualWidth;
            ConfigService.Instanse.MainWindowState = this.WindowState;
            ConfigService.Instanse.ColumnSizes = size.ToString().TrimEnd(',');

            ConfigService.Save();

        }
    }




    public partial class KeblerWindow
    {
        private void Instance_LangChanged()
        {
            IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                               $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.uploadedBytes)}" +
                               $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                               $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";
        }

        public void InitConnection()
        {
            if (IsConnected) return;

            if (ServersList.Count == 0)
            {
                ShowConnectionManager();
            }
            else
            {
                if (_checkerTask == null)
                {
                    _checkerTask = GetLongChecker();
                    _checkerTask.Start();
                }
                new Task(() => { TryConnect(ServersList.FirstOrDefault()); }).Start();

            }
        }

        private async void TryConnect(Server server)
        {
            if (server == null) return;

            Log.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = Kebler.Resources.Windows.MW_ConnectingText;
            try
            {
                IsConnecting = true;
                var pass = string.Empty;
                if (server.AskForPassword)
                {
                    Log.Info("Manual ask password");

                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass))
                    {
                        //await Application.Current.Dispatcher.InvokeAsync(() =>
                        //{
                        //    var dd = new Windows.MessageBox(Resources.Windows.MW_PAsswordCantBeEmpty);
                        //    dd.Show(App.Current.MainWindow);
                        //    //dd.ShowDialog(App.Current.MainWindow);
                        //});

                        IsErrorOccuredWhileConnecting = true;
                        return;
                    }
                }

                try
                {
                    _transmissionClient = new TransmissionClient(server.FullUriPath, null, server.UserName, server.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(server.Password));
                    Log.Info("Client created");
                    ConnectedServer = server;
                    StartCycle(server);
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

        private Task GetLongChecker()
        {
            return new Task(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Dispatcher.HasShutdownStarted)
                        {
                            throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");
                        }

                        var date = DateTimeOffset.Now;
                        var time = (date - _longActionTimeStart);
                        if (time.TotalSeconds > 2 && _isLongTaskRunning)
                        {
                            IsDoingStuff = true;
                        }
                    }
                    catch(TaskCanceledException)
                    {
                        return;
                    }
                    catch
                    {

                    }
                    await Task.Delay(1000);
                }
            });
        }

        void StartCycle(Server server)
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
                    _sessionInfo = await UpdateSessionInfo();
                    SelectedServer = server;
                    IsConnected = true;

                    Log.Info($"Connected {_sessionInfo.Version}");

                    while (IsConnected && !token.IsCancellationRequested)
                    {

                        if (Application.Current?.Dispatcher != null && Application.Current.Dispatcher.HasShutdownStarted)
                            return;

                        _stats = await UpdateStats();

                        Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;

                        IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.uploadedBytes)}" +
                                $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                        //sessionInfo = await UpdateSessionInfo();
                        _settings = await UpdateSettings();


                        if (_settings?.AlternativeSpeedEnabled != null)
                            IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;


                        var newTorrentsDataList = await UpdateTorrentList();

                        allTorrents = newTorrentsDataList.Torrents;


                        //UpdateTorrentListAtUi(ref torrents);
                        UpdateCategories(newTorrentsDataList.Torrents.Select(x => x.DownloadDir).ToList());
                        UpdateSorting(newTorrentsDataList);
                        UpdateStatsInfo();

                        await Task.Delay(5000, token);
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

        public void ShowConnectionManager()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _cmWindow = new ConnectionManager(ref _dbServers);
                _cmWindow.Owner = Application.Current.MainWindow;
                _cmWindow.ShowDialog();
            });
        }

        private static async Task<string> GetPassword()
        {
            var pass = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new MessageBox(true, "Enter password", true);
                dialog.Owner = Application.Current.MainWindow;
                try
                {
                    if (dialog.ShowDialog() == true)
                    {
                        pass = dialog.Value;
                    }
                }
                finally
                {

                    dialog.Value = null;
                    dialog = null;
                }
            });
            return pass;
        }

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

            var torrents = new List<TorrentInfo>(TorrentList);
            //1: 'check pending',
            //2: 'checking',

            //5: 'seed pending',
            //6: 'seeding',
            switch (_filterCategory)
            {
                //3: 'download pending',
                //4: 'downloading',
                case Kebler.Models.Enums.Categories.Downloading:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 3 || x.Status == 4).ToList();
                    break;

                //4: 'downloading',
                //6: 'seeding',
                //2: 'checking',
                case Kebler.Models.Enums.Categories.Active:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToList();
                    torrentsToSort = torrentsToSort.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToList();
                    break;

                //0: 'stopped' and is error,
                case Kebler.Models.Enums.Categories.Stopped:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString)).ToList();
                    break;

                case Kebler.Models.Enums.Categories.Error:
                    torrentsToSort = torrentsToSort.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToList();
                    break;


                //4: 'downloading',
                //6: 'seeding',
                //2: 'checking',
                case Kebler.Models.Enums.Categories.Inactive:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToList();
                    torrentsToSort = torrentsToSort.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0).ToList();
                    break;

                //6: 'seeding',
                case Kebler.Models.Enums.Categories.Ended:
                    torrentsToSort = torrentsToSort.Where(x => x.Status == 6).ToList();
                    break;
                case Kebler.Models.Enums.Categories.All:
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
            return await ExecuteLongTask(_transmissionClient.TorrentGetAsync, Kebler.Resources.Windows.MW_StatusText_Torrents);
        }

        private async Task<Statistic> UpdateStats()
        {
            return await ExecuteLongTask(_transmissionClient.GetSessionStatisticAsync, Kebler.Resources.Windows.MW_StatusText_Stats);
        }

        private async Task<SessionSettings> UpdateSettings()
        {
            return await ExecuteLongTask(_transmissionClient.GetSessionSettingsAsync, Kebler.Resources.Windows.MW_StatusText_Settings);
        }

        private async Task<SessionInfo> UpdateSessionInfo()
        {
            return await ExecuteLongTask(_transmissionClient.GetSessionInformationAsync, Kebler.Resources.Windows.MW_StatusText_Session);
        }

        private async Task<dynamic> ExecuteLongTask(Func<dynamic> asyncTask, string longStatusText)
        {
            try
            {
                _isLongTaskRunning = true;
                _longActionTimeStart = DateTimeOffset.Now;
                LongStatusText = longStatusText;

                while (true)
                {
                    // Debug.WriteLine($"Start getting {nameof(asyncTask)}");
                    if (Dispatcher == null || Dispatcher.HasShutdownStarted)
                    {
                        throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");
                    }

                    var resp = await asyncTask();
                    if (resp == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }
                    return resp;
                }
            }
            finally
            {
                IsDoingStuff = false;
                LongStatusText = string.Empty;
                _isLongTaskRunning = false;
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

                        //if (SelectedTorrent != null && SelectedTorrent.ID == data.ID) SelectedTorrent = null;

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
            //if (SelectedTorrent == null) return;
            //if (SelectedTorrent.ID == newData.ID) SelectedTorrent = newData;

            var myType = typeof(TorrentInfo);
            var props = myType.GetProperties().Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);

            Validate(ref newData);
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

        public void Validate(ref TorrentInfo torrInf)
        {
            if (torrInf.Status == 1 || torrInf.Status == 2)
            {
                torrInf.PercentDone = torrInf.RecheckProgress;
                return;
            }

            if (torrInf.Status == 0)
                return;
            //if (torrInf.Error != 0)
            //{
            //    torrInf.Status = -1; //assume about error
            //}

            if (torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false))
            {
                torrInf.Status = -1;
            }
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


        #endregion

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
