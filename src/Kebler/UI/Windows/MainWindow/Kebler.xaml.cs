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
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Attributes;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;
using Kebler.Models.Torrent.Response;
using Kebler.Resources;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.TransmissionCore;
using Kebler.UI.Dialogs;
using Microsoft.Win32;
using Newtonsoft.Json;
using Enums = Kebler.Models.Enums;

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

            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_AllTorrents, Cat = Enums.Categories.All });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Downloading, Cat = Enums.Categories.Downloading });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Active, Cat = Enums.Categories.Active });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_InActive, Cat = Enums.Categories.Inactive });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Ended, Cat = Enums.Categories.Ended });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Stopped, Cat = Enums.Categories.Stopped });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Error, Cat = Enums.Categories.Error });

            CategoriesListBox.SelectedIndex = 0;
        }

        private void KeblerWindow_OnActivated(object sender, EventArgs e)
        {
            Init_HK();
        }

        private void KeblerWindow_OnDeactivated(object sender, EventArgs e)
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
                TorrentFields.TRACKER_STATS,
                TorrentFields.DOWNLOAD_DIR,

                //TODO: REMOVE 
                //TorrentFields.PIECE_COUNT,
                //TorrentFields.PIECES
            };
            try
            {
                
                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                Width = ConfigService.Instanse.MainWindowWidth;
                Height = ConfigService.Instanse.MainWindowHeight;
                WindowState = (System.Windows.WindowState)ConfigService.Instanse.MainWindowState;
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

            Log.Info("-----------Exit-----------");
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
                var dialog = new AddTorrentDialog(item, ref _transmissionClient) { Owner = this };

                if (!(bool)dialog.ShowDialog())
                    return;

                if (dialog.TorrentResult.Value.Status != Enums.AddTorrentStatus.Added) continue;

                TorrentList.Add(new TorrentInfo(dialog.TorrentResult.Value.ID)
                {
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

            _transmissionClient.TorrentStopAsync(SelectedTorrents.Select(x => x.Id).ToArray());
        }

        private async void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var torrentIds = TorrentList.Select(x => x.Id).ToArray();
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

            var torrentIds = TorrentList.Select(x => x.Id).ToArray();
            await Task.Factory.StartNew(() =>
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
                _transmissionClient.TorrentStartAsync(SelectedTorrents.Select(x => x.Id).ToArray());
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

        public void RemoveTorrent(bool removeData = false)
        {
            var toRemove = SelectedTorrents.Select(x => x.Id).ToArray();
            var dialog = new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), toRemove, ref _transmissionClient, removeData) { Owner = this };
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.Result == Enums.RemoveResult.Ok)
                {
                    lock (_syncTorrentList)
                    {
                        foreach (var rm in toRemove)
                        {
                            var itm = TorrentList.First(x => x.Id == rm);
                            TorrentList.Remove(itm);

                            allTorrents.Torrents = allTorrents.Torrents.Where(val => val.Id == rm).ToArray();
                        }
                        if (allTorrents.Clone() is TransmissionTorrents data)
                        {
                            ProcessTorrentData(data);
                        }
                      
                    }

                }
            }
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

            lock (_syncTorrentList)
            {
                _filterCategory = ((StatusCategory)listBox.SelectedValue).Cat;
            }

            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessTorrentData(data);
            }
        }

        private async void SetLocation_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var items = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToList();
            var question = items.Count > 1 ? Kebler.Resources.Dialogs.SetLocForMany.Replace("%d", items.Count.ToString())
                : Kebler.Resources.Dialogs.SetLocOnce;

            var path = items.First().DownloadDir;

            var dialog = new DialogBox(question, path, false) { Owner = Application.Current.MainWindow };

            if (dialog.ShowDialog() == true)
            {
                await Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (Dispatcher.HasShutdownStarted) return;
                        var resp = await _transmissionClient.TorrentSetLocationAsync(SelectedTorrents.Select(x => x.Id).ToArray(), dialog.Value, true, _cancelTokenSource.Token);

                        if (CheckResponse(resp))
                            break;

                        await Task.Delay(500, _cancelTokenSource.Token);
                    }
                }, _cancelTokenSource.Token);
            }

            dialog.Value = null;
        }

        private void ChangeFolderFilter(object sender, MouseButtonEventArgs e)
        {
            if (FolderListBox.SelectedItem is FolderCategory cat)
            {
                FilterTextBox.Text = $"{{p}}:{cat.FullPath}";
            }
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            FilterText = FilterTextBox.Text;
            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessTorrentData(data);
            }
        }

        private void ClearFilter(object sender, MouseButtonEventArgs e)
        {
            FilterTextBox.Clear();
            FolderListBox.SelectedIndex = -1;
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

                if(!string.IsNullOrEmpty(FilterText) && FilterText.Contains("{p}:"))
                {
                    var splited = FilterText.Split("{p}:");
                    var filterKey = splited[^1];
                    data.Torrents = data.Torrents.Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToArray();
                }


                //add or remove torrents to grid
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
                            Log.Info($"Removed torrent from UI Grid'{item}'");
                        }
                    }




                    foreach (var item in TorrentList)
                    {
                        UpdateTorrentData(TorrentList.IndexOf(item), data.Torrents.First(x => x.Id == item.Id));
                    }

                    //add
                    if (toAdd.Length > 0)
                    {
                        Debug.WriteLine($"toAdd {toAdd.Length}");
                        foreach (var item in toAdd)
                        {
                            var dt = Validate(item, true);
                            Dispatcher.Invoke(() => { TorrentList.Add(dt); });
                            Log.Info($"Added torrent to UI Grid '{dt}'");
                        }
                    }



                    toAdd = null;
                    toRm = null;


                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }


                UpdateCategories(allTorrents.Torrents.Select(x => new FolderCategory(x.DownloadDir)).ToList());



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
                               $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
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


                    if (CheckResponse(info))
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


                            allTorrents = await ExecuteLongTask(_transmissionClient.TorrentGetAsync,
                                Kebler.Resources.Windows.MW_StatusText_Torrents, WorkingParams);



                            ParseSettings();
                            ParseStats();

                            if (allTorrents.Clone() is TransmissionTorrents data)
                            {
                                ProcessTorrentData(data);
                            }

                            await Task.Delay(5000, token);
                        }
                    }
                    else
                    {
                        if (info.WebException != null)
                            throw info.WebException;
                        if (info.CustomException != null)
                            throw info.CustomException;

                        throw new Exception(info.HttpWebResponse.StatusCode.ToString());
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    if (IsConnected)
                    {
                        await _transmissionClient.CloseSessionAsync();
                    }
                 
                    allTorrents = null;
                    TorrentList = new System.Collections.ObjectModel.ObservableCollection<TorrentInfo>();
                    IsConnected = false;
                    IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                    Log.Info("Disconnected from server");
                }
            }, token);

            _whileCycleTask.Start();
        }


        private bool CheckResponse(TransmissionResponse resp)
        {

            if (resp.WebException != null)
            {
                Dispatcher.Invoke(() =>
                {
                    var msg = resp.WebException.Status switch
                    {
                        System.Net.WebExceptionStatus.NameResolutionFailure => $"{Kebler.Resources.Dialogs.EX_Host} '{SelectedServer.Host}'",
                        _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                    };

                    var dialog = new MessageBox(msg, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true) { Owner = this };
                    dialog.ShowDialog();
                });
                return false;
            }

            if (resp.CustomException != null)
            {

            }
            return true;
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
                    $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                    $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                    $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.DownloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.UploadSpeed);

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

        private void UpdateCategories(List<FolderCategory> dirrectories)
        {
            if (dirrectories == null) return;
            Debug.WriteLine("Update Cats");
            var dirs = dirrectories.Distinct().ToList();

            var cats = new List<FolderCategory>();

            foreach (var cat in dirs)
            {

                cat.Title += $"{cat.FolderName} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == cat.FullPath)})";
                cats.Add(cat);
            }


            var toRm = Categories.Except(cats).ToList();
            var toAdd = cats.Except(Categories).ToList();

            if (toRm.Any())
            {
                Log.Info($"Remove categories" + string.Join(", ", toRm));

                foreach (var itm in toRm)
                {
                    Dispatcher.Invoke(() => { Categories.Remove(itm); });
                }
            }


            Dispatcher.Invoke(() =>
            {
                foreach (var itm in Categories)
                {
                    itm.Title += $"{itm.FolderName} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                }
            });

            if (toAdd.Any())
            {
                Log.Info($"Add categories" + string.Join(", ", toAdd));
                foreach (var itm in toAdd)
                {

                    Dispatcher.Invoke(() => { Categories.Add(itm); });
                }
            }


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


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



    }
}
