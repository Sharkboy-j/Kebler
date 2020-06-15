using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Kebler.Models;
using Kebler.Resources;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.UI.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;
using Transmission.API.RPC.Response;
using Enums = Kebler.Models.Enums;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow : INotifyPropertyChanged
    {
        ICommand Add;
        public KeblerWindow()
        {


            InitializeComponent();

            ApplyConfig();

            DataContext = this;

            App.Instance.LangChanged += Instance_LangChanged;

            Task.Factory.StartNew(InitConnection);

            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_AllTorrents, Tag = Enums.Categories.All });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_Downloading, Tag = Enums.Categories.Downloading });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_Active, Tag = Enums.Categories.Active });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_InActive, Tag = Enums.Categories.Inactive });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_Ended, Tag = Enums.Categories.Ended });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_Stopped, Tag = Enums.Categories.Stopped });
            CategoriesListBox.Items.Add(new ListBoxItem { Name = Strings.Cat_Error, Tag = Enums.Categories.Error });

            CategoriesListBox.SelectedIndex = 0;
        }

        private void KeblerWindow_OnActivated(object? sender, EventArgs e)
        {
            Init_HK();
        }

        private void KeblerWindow_OnDeactivated(object? sender, EventArgs e)
        {
            UnregisterHotKeys();
        }

        private void Init_HK()
        {

            lock (syncObjKeys)
            {
                if (RegisteredKeys == null)
                    RegisteredKeys = new[]
                    {
                        new HotKey(Key.C, KeyModifier.Shift | KeyModifier.Ctrl, ShowConnectionManager, this.GetWindowHandle()),
                        new HotKey(Key.N, KeyModifier.Ctrl, AddTorrent, this.GetWindowHandle())
                    };
            }
        }

        private void UnregisterHotKeys()
        {
            lock (syncObjKeys)
            {
                foreach (var key in RegisteredKeys)
                {
                    key.Dispose();
                }
                RegisteredKeys = null;
            }
        }


        private void ApplyConfig()
        {
            WorkingParams = new[]
            {
                TorrentFields.NAME,
                TorrentFields.ID,
                TorrentFields.ADDED_DATE,
                TorrentFields.HASH_STRING,
                TorrentFields.RATE_DOWNLOAD,
                TorrentFields.RATE_UPLOAD,
                TorrentFields.RECHECK,
                TorrentFields.PERCENT_DONE,
                TorrentFields.UPLOADED_EVER,
                TorrentFields.STATUS,
                TorrentFields.TRACKER_STATS
            };
            try
            {
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                Width = ConfigService.Instanse.MainWindowWidth;
                Height = ConfigService.Instanse.MainWindowHeight;
                WindowState = ConfigService.Instanse.MainWindowState;
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                //if (!string.IsNullOrEmpty(ConfigService.Instanse.ColumnSizes))
                //{
                //    var splited = ConfigService.Instanse.ColumnSizes.Split(',');
                //    for (int i = 1; i < splited.Length; i++)
                //    {
                //        TorrentsDataGrid.Columns[i].Width = Convert.ToDouble(splited[i]);
                //    }
                //}


            }
            catch (Exception ex)
            {
                Log.Error(ex);

                var msg = new MessageBox(Kebler.Resources.Dialogs.ConfigApllyError, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true);
                msg.ShowDialog();
            }
        }

        private void Vertical_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ConfigService.Instanse.CategoriesWidth = CategoriesColumn.ActualWidth;
            ConfigService.Save();
        }

        private void ClosingW(object sender, CancelEventArgs e)
        {
            var size = new StringBuilder();
            foreach (var column in TorrentsDataGrid.Columns)
            {
                size.Append($"{column.ActualWidth},");
            }


            ConfigService.Instanse.MainWindowHeight = ActualHeight;
            ConfigService.Instanse.MainWindowWidth = ActualWidth;
            ConfigService.Instanse.MainWindowState = WindowState;
            //ConfigService.Instanse.ColumnSizes = size.ToString().TrimEnd(',');

            ConfigService.Save();

        }



        #region ToolBar actions
        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(IEnumerable<string> names)
        {
            foreach (var item in names)
            {
                var dialog = new AddTorrentDialog(item, _settings, ref _transmissionClient) { Owner = this };

                if (!(bool)dialog.ShowDialog())
                    return;

                if (dialog.TorrentResult.Value.Status != Transmission.API.RPC.Entity.Enums.AddTorrentStatus.Added) continue;

                TorrentList.Add(new TorrentInfo
                {
                    ID = dialog.TorrentResult.Value.ID,
                    Name = dialog.TorrentResult.Value.Name,
                    HashString = dialog.TorrentResult.Value.HashString
                });

                OnPropertyChanged(nameof(TorrentList));
            }
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            IsErrorOccuredWhileConnecting = false;
            UpdateServers();
            InitConnection();
        }

        private void TorrentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTorrents = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToArray();
        }



        private void PauseTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            _transmissionClient.TorrentStopAsync(SelectedTorrents.Select(x => x.ID).ToArray());
        }

        private async void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var torrentIds = TorrentList.Select(x => x.ID).ToArray();
            await Task.Factory.StartNew(() =>
            {
                Log.Info("Try start all torrents");

                _transmissionClient.TorrentStartAsync(torrentIds);

                Log.Info("Started all");
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
                Log.Info("Try pause all torrents");

                _transmissionClient.TorrentStopAsync(torrentIds);

                Log.Info("Paused all");
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
            var toRemove = SelectedTorrents.Select(x => x.ID).ToArray();
            var obj = new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), removeData);
            var dialog =
                new MessageBox(obj, true, string.Empty, Enums.MessageBoxDilogButtons.OkCancel, true) { Owner = this };


            if ((bool)dialog.ShowDialog())
            {
                if (!IsConnected)
                    new MessageBox(Kebler.Resources.Dialogs.ConnectionLost, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true).Show();

                var data = obj.WithData;
                await Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (Dispatcher.HasShutdownStarted)
                            return;
                        var exitCondition = await _transmissionClient.TorrentRemoveAsync(toRemove, data);

                        if (exitCondition == Transmission.API.RPC.Entity.Enums.RemoveResult.Ok)
                            break;
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
            _transmissionClient.SetSessionSettingsAsync(new SessionSettings { AlternativeSpeedEnabled = !_settings.AlternativeSpeedEnabled });
        }

        #endregion


        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || !IsConnected)
                return;
            if (!(sender is ListBox listBox)) return;

            _filterCategory = (Enums.Categories)((ListBoxItem)listBox.SelectedValue).Tag;
            var data = allTorrents.Clone() as TransmissionTorrents;
            ProcessTorrentData(data);
        }

        private async void SetLocation_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;


            var dialog = new MessageBox(true, "Please, enter new location");
            dialog.Owner = Application.Current.MainWindow;
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    await Task.Factory.StartNew(async () =>
                    {

                        var exitCondition = Transmission.API.RPC.Entity.Enums.RemoveResult.Error;
                        while (exitCondition != Transmission.API.RPC.Entity.Enums.RemoveResult.Ok)
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


    }



















    public partial class KeblerWindow
    {


        private void ProcessTorrentData(TransmissionTorrents data)
        {
            if (!IsConnected)
                return;

            lock (_syncTorrentList)
            {
                //var torrents = new List<TorrentInfo>(TorrentList);
                ////1: 'check pending',
                ////2: 'checking',

                ////5: 'seed pending',
                ////6: 'seeding',
                switch (_filterCategory)
                {
                    //3: 'download pending',
                    //4: 'downloading',
                    case Enums.Categories.Downloading:
                        data.Torrents = data.Torrents.Where(x => x.Status == 3 || x.Status == 4).ToArray();
                        break;

                    //4: 'downloading',
                    //6: 'seeding',
                    //2: 'checking',
                    case Enums.Categories.Active:
                        data.Torrents = data.Torrents.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2).ToArray();
                        data.Torrents = data.Torrents.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
                        break;

                    //0: 'stopped' and is error,
                    case Enums.Categories.Stopped:
                        data.Torrents = data.Torrents.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;

                    case Enums.Categories.Error:
                        data.Torrents = data.Torrents.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;


                    //6: 'seeding',
                    //1: 'checking queue',
                    case Enums.Categories.Inactive:
                        //var array1 = data.Torrents.Where(x=> x.Status == 1).ToArray();
                        var array2 = data.Torrents.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2).ToArray();

                        //int array1OriginalLength = array1.Length;
                        //Array.Resize<TorrentInfo>(ref array1, array1OriginalLength + array2.Length);
                        //Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
                        data.Torrents = array2;
                        break;

                    //6: 'seeding',
                    case Enums.Categories.Ended:
                        data.Torrents = data.Torrents.Where(x => x.Status == 6).ToArray();
                        break;
                    case Enums.Categories.All:
                    default:
                        break;
                }

                try
                {

                    var toRm = TorrentList.Except(data.Torrents).ToArray();
                    var toAdd = data.Torrents.Except(TorrentList).ToArray();

                    //remove
                    if (toRm.Length > 0)
                    {
                        Debug.WriteLine($"toRm {toRm.Length}");
                        foreach (var item in toRm)
                        {
                            Dispatcher.Invoke(() => { TorrentList.Remove(item); });
                        }
                    }
                    toRm = null;

                    foreach (var item in TorrentList)
                    {
                        UpdateTorrentData(TorrentList.IndexOf(item), data.Torrents.First(x => x.ID == item.ID));
                    }

                    //add
                    if (toAdd.Length > 0)
                    {
                        Debug.WriteLine($"toAdd {toAdd.Length}");
                        foreach (var item in toAdd)
                        {
                            var dt = Validate(item, true);
                            Dispatcher.Invoke(() => { TorrentList.Add(dt); });
                        }
                    }
                    toAdd = null;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }


                //data.Torrents = data.Torrents.OrderBy(x => x.Status).ToArray();

                //var selected = SelectedTorrents.Select(x => x.ID).ToArray();

                //for (int i = 0; i < data.Torrents.Length; i++)
                //{

                //    var item = TorrentList.First(x => x.ID == data.Torrents[i].ID);
                //    Dispatcher.Invoke(() =>
                //    {
                //        TorrentList.Move(TorrentList.IndexOf(item), i);
                //    });
                //}

                Dispatcher.Invoke(() =>
                {
                    var column = TorrentsDataGrid.Columns.Last();

                    // Clear current sort descriptions
                    TorrentsDataGrid.Items.SortDescriptions.Clear();

                    // Add the new sort description
                    TorrentsDataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, ListSortDirection.Descending));

                    // Apply sort
                    foreach (var col in TorrentsDataGrid.Columns)
                    {
                        col.SortDirection = null;
                    }
                    column.SortDirection = ListSortDirection.Ascending;

                    // Refresh items to display sort
                    TorrentsDataGrid.Items.Refresh();
                });


            }
        }




























































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

                SelectedServer ??= ServersList.FirstOrDefault();
                //new Task(() => {  }).Start();
                TryConnect();

            }
        }

        private async void TryConnect()
        {
            if (SelectedServer == null) return;

            Log.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = Kebler.Resources.Windows.MW_ConnectingText;
            try
            {
                IsConnecting = true;
                var pass = string.Empty;
                if (SelectedServer.AskForPassword)
                {
                    Log.Info("Manual ask password");

                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass))
                    {
                        IsErrorOccuredWhileConnecting = true;
                        return;
                    }
                }

                try
                {
                    _transmissionClient = new TransmissionClient(SelectedServer.FullUriPath, null, SelectedServer.UserName,
                        SelectedServer.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password));
                    Log.Info("Client created");
                    ConnectedServer = SelectedServer;
                    StartCycle();
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
                    catch (TaskCanceledException)
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

        void StartCycle()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;

            _whileCycleTask = new Task(async () =>
            {
                try
                {
                    var info = await UpdateSessionInfo();
                    if (info.Error == Transmission.API.RPC.Entity.Enums.ErrorsResponse.None)
                    {
                        _sessionInfo = info.Value;
                        IsConnected = true;
                        Log.Info($"Connected {_sessionInfo.Version}");

                        while (IsConnected && !token.IsCancellationRequested)
                        {

                            if (Application.Current?.Dispatcher != null &&
                                Application.Current.Dispatcher.HasShutdownStarted)
                                throw new TaskCanceledException();

                            _stats = await ExecuteLongTask(_transmissionClient.GetSessionStatisticAsync,
                                Kebler.Resources.Windows.MW_StatusText_Stats);

                            //_settings = await ExecuteLongTask(_transmissionClient.GetSessionSettingsAsync, Kebler.Resources.Windows.MW_StatusText_Settings);

                            allTorrents = await ExecuteLongTask(_transmissionClient.TorrentGetAsync,
                                Kebler.Resources.Windows.MW_StatusText_Torrents, WorkingParams);



                            ParseSettings();
                            ParseStats();

                            var data = allTorrents.Clone() as TransmissionTorrents;
                            //UpdateCategories(newTorrentsDataList.Torrents.Select(x => x.DownloadDir).ToList());
                            ProcessTorrentData(data);
                            //GC.Collect();
                            //GC.WaitForPendingFinalizers();
                            await Task.Delay(5000, token);
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var msg = info.StatusError switch
                            {
                                System.Net.WebExceptionStatus.NameResolutionFailure => $"{Kebler.Resources.Dialogs.EX_Host} '{SelectedServer.Host}'",
                                _ => $"{info.StatusError} {Environment.NewLine} {info.Exception?.Message}"
                            };

                            var dialog = new MessageBox(msg, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true) { Owner = this };
                            dialog.ShowDialog();
                        });
                    }


                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    _transmissionClient.CloseSessionAsync();
                    allTorrents = null;
                    TorrentList = new System.Collections.ObjectModel.ObservableCollection<TorrentInfo>();
                    IsConnected = false;
                    IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                    Log.Info("Disconnected from server");
                }
            }, token);

            _whileCycleTask.Start();
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            IsConnected = false;
            _cancelTokenSource.Cancel();
            _whileCycleTask.Wait();
            _whileCycleTask.Dispose();
            _whileCycleTask = null;
        }

        public async void ReconnectToNewServer()
        {
            Log.Info("Try connect to another server");
            Disconnect();
            await Task.Delay(1500);
            InitConnection();
        }

        public void ParseSettings()
        {

            if (_settings?.AlternativeSpeedEnabled != null)
                IsSlowModeEnabled = (bool)_settings.AlternativeSpeedEnabled;
        }

        public void ParseStats()
        {
            Thread.CurrentThread.CurrentUICulture = LocalizationManager.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = LocalizationManager.CurrentCulture;

            IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                    $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.uploadedBytes)}" +
                    $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                    $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.downloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.uploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;

            DownloadSpeed = $"D: {dSpeed}";
            UploadSpeed = $"U: {uSpeed}";
        }

        public void ShowConnectionManager()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _cmWindow = new ConnectionManager(ref _dbServers) { Owner = Application.Current.MainWindow };
                _cmWindow.ShowDialog();
                UpdateServers();
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
            //if (dirrectories == null) return;

            //var dirs = dirrectories.Distinct().ToList();


            //foreach (var path in dirs)
            //{
            //    var cat = new Category(path);
            //    cat.Title += $"{cat.FolderName} ({allTorrents.Count(x => x.DownloadDir == cat.FullPath)})";
            //    if (!IsExist(Categories, cat))
            //        Dispatcher.Invoke(() =>
            //        {
            //            Categories.Add(cat);
            //        });
            //}
        }

        public bool IsExist(IEnumerable<Category> list, Category item)
        {
            return list.Any(x => x.FolderName == item.FolderName);
        }

        #region ServerActions


        private async Task<TransmissionTorrents> ExecuteLongTask(Func<string[], Task<TransmissionTorrents>> asyncTask, string longStatusText, string[] args)
        {

            try
            {
                _isLongTaskRunning = true;
                _longActionTimeStart = DateTimeOffset.Now;
                LongStatusText = longStatusText;

                while (true)
                {
                    if (Dispatcher == null || Dispatcher.HasShutdownStarted)
                    {
                        throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");
                    }

                    var resp = await asyncTask.Invoke(args);
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



        private async Task<TransmissionInfoResponse<SessionInfo>> UpdateSessionInfo()
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


        private void UpdateTorrentData(int oldIndex, TorrentInfo newData)
        {

            var myType = typeof(TorrentInfo);
            var props = myType.GetProperties().Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0);
            props = props.Where(p => p.GetCustomAttributes(typeof(SetIgnoreAttribute), true).Length == 0);

            var trnt = Validate(newData);
            foreach (var item in props)
            {

                var name = item.GetCustomAttribute(typeof(JsonPropertyAttribute));



                if (WorkingParams.Contains(((JsonPropertyAttribute)name).PropertyName))
                {
                    var newVal = trnt.Get(item.Name);
                    TorrentList[oldIndex].Set(item.Name, newVal);
                }
            }
        }


        public TorrentInfo Validate(TorrentInfo torrInf, bool skip = false)
        {
            if (torrInf.Status == 1 || torrInf.Status == 2)
            {
                torrInf.PercentDone = torrInf.RecheckProgress;
                return torrInf;
            }

            if (torrInf.Status == 0)
                return torrInf;

            if (!skip)
                if (torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false))
                {
                    torrInf.Status = -1;
                }
            return torrInf;

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
