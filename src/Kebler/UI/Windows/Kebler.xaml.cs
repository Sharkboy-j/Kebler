using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Transmission.API.RPC.Entity;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Kebler.UI.ViewModels;
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

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="KeblerWindow" />
    /// <summary>
    /// Interaction logic for Kebler.xaml
    /// </summary>
    public partial class KeblerWindow : INotifyPropertyChanged
    {


        public void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        public void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        public void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
        public void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
        public void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
        public void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
        public void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
        public void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }

        public void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Normal)
                    DragSize(this.GetWindowHandle(), action);
            }
        }

        public void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                this.Close();
            }
            else
            {
                SendMessage(this.GetWindowHandle(), WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)' ');
            }
        }

        public void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MinButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        public void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ClickCount > 1 && this.ResizeMode != ResizeMode.NoResize)
            {
                MaxButtonClick(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        public void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (this.WindowState == WindowState.Maximized)
                {
                    this.BeginInit();
                    double adjustment = 40.0;
                    var mouse1 = e.MouseDevice.GetPosition(this);
                    var width1 = Math.Max(this.ActualWidth - 2 * adjustment, adjustment);
                    this.WindowState = WindowState.Normal;
                    var width2 = Math.Max(this.ActualWidth - 2 * adjustment, adjustment);
                    this.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                    this.Top = -7;
                    this.EndInit();
                    this.DragMove();
                }

            }
        }



        #region P/Invoke

        const int WM_SYSCOMMAND = 0x112;
        const int SC_SIZE = 0xF000;
        const int SC_KEYMENU = 0xF100;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        public enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }

        #endregion



        public KeblerWindow()
        {
            InitializeComponent();

            //disable hardware rendering
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            DataContext = this;

            App.Instance.LangChanged += Instance_LangChanged;

            Task.Factory.StartNew(InitConnection);

        }


        public void UpdateSorting()
        {
            UpdateSorting();
        }



        public void OpenConnectionManager()
        {
            ShowConnectionManager();
        }

        public void Connect()
        {
            InitConnection();
        }



        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true) return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(string[] names)
        {
            //foreach (var item in names)
            //{
            //    var dialog = new AddTorrentDialog(item, Vm._settings, ref Vm._transmissionClient);
            //    if (ConfigService.Instanse.IsAddTorrentWindowShow)
            //    {
            //        dialog.Add(null, null);
            //    }
            //    else
            //    {
            //        if (!(bool)dialog.ShowDialog())
            //            return;
            //    }
            //}
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            IsErrorOccuredWhileConnecting = false;
            InitConnection();
        }

        private void TorrentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (TorrentsDataGrid.SelectedValue is TorrentInfo tor)
            //{
            //    Vm.SelectedTorrent = tor;
            //}
        }

        private void RemoveTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent();
        }
        private void RemoveTorrentData_ItemClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent(true);
        }

        private void PauseTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            PauseTorrent();
        }

        private void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            StartAll();
        }

        private void AddTorrentButtonClick(object sender, RoutedEventArgs e)
        {
            AddTorrent();
        }

        private void PauseAll_ButtonClick(object sender, RoutedEventArgs e)
        {
            StopAll();
        }

        private void StartSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            StartOne();
        }

        private void PauseSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            PauseTorrent();
        }

        private void RemoveTorrent_ButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent();
        }
        private void RemoveTorrentWithData_ButtonClick(object sender, RoutedEventArgs e)
        {
            RemoveTorrent(true);
        }


        private void Window_StateChanged_1(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            if (!(sender is System.Windows.Controls.ListBox listBox)) return;

            if (listBox.SelectedItem is Category catObj)
                ChangeFilterType(catObj.Tag);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SlowMode();
        }

        private void ListBox_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (CategoriesListBox.SelectedItem == null)
            //    return;

            //if (CategoriesListBox.SelectedItem is Category cat)
            //{
            //    FilterTextBox.Text = $"{{p}}:{cat.FullPath}";
            //}
        }

        private void Border_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //FilterTextBox.Clear();
            //CategoriesListBox.SelectedIndex = -1;
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!IsLoaded)
            //    return;
            //Vm.FilterText = FilterTextBox.Text;
            //Vm.UpdateSorting();

        }

        private void SetLocation_OnClick(object sender, RoutedEventArgs e)
        {
            SetLocation();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dd = new Windows.MessageBox(new About());
            dd.ShowDialog(this);
        }

    }

    public partial class KeblerWindow
    {
        private List<Server> _servers;
        private List<Server> ServersList
        {
            get
            {
                if (_servers == null)
                {
                    _dbServers = StorageRepository.GetServersList();
                    _servers = _dbServers.FindAll().ToList();
                }
                return _servers;
            }
        }
        private LiteCollection<Server> _dbServers;
        private Statistic _stats;
        private SessionInfo _sessionInfo;
        public TransmissionClient _transmissionClient;
        private ConnectionManager _cmWindow;
        private Task _whileCycleTask;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private Category.Categories _filterCategory = Category.Categories.All;
        private DateTimeOffset _longActionTimeStart;
        private bool _isLongTaskRunning;
        private Task _checkerTask;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public Server SelectedServer { get; set; }
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


        #region StatusBarProps
        public string IsConnectedStatusText { get; set; } = string.Empty;
        public string DownloadSpeed { get; set; } = string.Empty;
        public string UploadSpeed { get; set; } = string.Empty;


        #endregion



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
                    if (Dispatcher.HasShutdownStarted)
                    {
                        throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");
                    }
                    try
                    {
                        var date = DateTimeOffset.Now;
                        var time = (date - _longActionTimeStart);
                        if (time.TotalSeconds > 2 && _isLongTaskRunning)
                        {
                            IsDoingStuff = true;
                        }
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

        public void ChangeFilterType(Category.Categories cat)
        {
            _filterCategory = cat;
            UpdateSorting();
        }


        public async void SetLocation()
        {
            if (!IsConnected) return;

            var torrentInfo = SelectedTorrent;

            var dialog = new MessageBox(true, "Please, enter new location", false);
            dialog.Owner = Application.Current.MainWindow;
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        Log.Info($"Try set location {torrentInfo.Name}({torrentInfo.DownloadDir} => {dialog.Value})");

                        var exitCondition = Enums.RemoveResult.Error;
                        while (exitCondition != Enums.RemoveResult.Ok)
                        {
                            if (Dispatcher.HasShutdownStarted) return;
                            _transmissionClient.TorrentSetLocationAsync(new[] { torrentInfo.ID }, dialog.Value, true);

                            await Task.Delay(500);
                        }
                        Log.Info($"Location set {torrentInfo.Name} true");
                    });
                }
            }
            finally
            {

                dialog.Value = null;
                dialog = null;
            }





        }

        public async void RemoveTorrent(bool removeData = false)
        {
            if (!IsConnected) return;

            var dialog = new MessageBox("You are perform to remove %n torrents with data", "Please, confirm action", Models.Enums.MessageBoxDilogButtons.YesNo, true, true);
            dialog.Owner = Application.Current.MainWindow;
            try
            {
                if (dialog.ShowDialog() == true)
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
            finally
            {
                dialog = null;
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
            if(torrInf.Status==1|| torrInf.Status==2)
            {
                torrInf.PercentDone = torrInf.RecheckProgress;
            }

            if (torrInf.Status == 0)
                return;
            if (torrInf.Error != 0)
            {
                torrInf.Status = -1; //assume about error
            }

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
