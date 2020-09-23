using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;
using Kebler.Resources;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.TransmissionCore;
using Kebler.Views;
using Microsoft.Win32;
using ILog = log4net.ILog;
using LogManager = log4net.LogManager;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : Screen,
        IHandle<Messages.LocalizationCultureChangesMessage>,
        IHandle<Messages.ReconnectRequested>,
        IHandle<Messages.ReconnectAllowed>,
        IHandle<Messages.ConnectedServerChanged>,
        IHandle<Messages.ServersUpdated>


    {
        private readonly IEventAggregator _eventAggregator;



        public KeblerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            InitMenu();

            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_AllTorrents, Cat = Enums.Categories.All });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_Downloading, Cat = Enums.Categories.Downloading });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_Active, Cat = Enums.Categories.Active });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_InActive, Cat = Enums.Categories.Inactive });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_Ended, Cat = Enums.Categories.Ended });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_Stopped, Cat = Enums.Categories.Stopped });
            CategoriesList.Add(new StatusCategory { Title = Strings.Cat_Error, Cat = Enums.Categories.Error });

            SelectedFolderIndex = -1;

            SelectedCat = CategoriesList.First();


            App.Instance.KeblerVM = this;


        }

        protected override void OnViewAttached(object view, object context)
        {
            _view = view as KeblerView;
            if (_view != null)
                _view.MoreView.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
            base.OnViewAttached(view, context);
            ApplyConfig();
        }

        private async void FileTreeViewControl_OnFileStatusUpdate(uint[] wanted, uint[] unwanted, bool status)
        {
            var settings = new TorrentSettings
            {
                IDs = selectedIDs,
                FilesWanted = wanted.Any() ? wanted : null,
                FilesUnwanted = unwanted.Any() ? unwanted : null
            };

            if (wanted.Length > 0)
                Log.Info("wanted " + string.Join(", ", wanted));

            if (unwanted.Length > 0)
                Log.Info("unwanted " + string.Join(", ", unwanted));

            var resp = await _transmissionClient.TorrentSetAsync(settings, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(InitConnection, cancellationToken);

            return base.OnActivateAsync(cancellationToken);
        }



        public void ShowConnectionManager()
        {
            manager.ShowDialogAsync(new ConnectionManagerViewModel(_eventAggregator));
        }

        private Task GetLongChecker()
        {
            return new Task(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Application.Current.Dispatcher.HasShutdownStarted)
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
                        // ignored
                    }

                    await Task.Delay(1000);
                }
            });
        }

        private async void StoppdateingMoreInfoCycle()
        {
            _moreInfoCancelTokeSource?.Cancel();
            if (_whileCycleMoreInfoTask != null)
            {
                await _whileCycleMoreInfoTask;
            }
            _whileCycleMoreInfoTask = null;

            //Debug.WriteLine("Stopped");
        }


        private void StartUpdateingMoreInfoCycle()
        {

            _moreInfoCancelTokeSource?.Cancel();

            _moreInfoCancelTokeSource = new CancellationTokenSource();
            var token = _moreInfoCancelTokeSource.Token;

            _whileCycleMoreInfoTask = new Task(async () =>
            {
                try
                {
                    while (IsConnected && !token.IsCancellationRequested)
                    {
                        await PerformMoreInfoUpdate(token);
                        await Task.Delay(ConfigService.Instanse.UpdateTime, token);
                    }
                }
                catch (TaskCanceledException)
                {

                }
                catch (Exception)
                {
                    //Log.Error(ex);
                }
            }, token);


            _whileCycleMoreInfoTask.Start();
            //Debug.WriteLine("Started");
        }



        #region TopBarMenu


        private void InitMenu()
        {
            foreach (var item in LocalizationManager.CultureList)
            {
                var dd = new MenuItem { Header = item.EnglishName, Tag = item };
                dd.Click += langChanged;
                dd.IsCheckable = true;
                dd.IsChecked = Equals(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, item.TwoLetterISOLanguageName);
                Languages.Add(dd);
            }

            ReInitServers();
        }


        public void Settings()
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {ConfigService.CONFIG_NAME}") { CreateNoWindow = true });
        }

        public void Preferences()
        {
            manager.ShowDialogAsync(new ServerPreferencesViewModel(_settings));
        }

        private void ReInitServers()
        {

            var _dbServersList = StorageRepository.GetServersList();

            var items = _dbServersList?.FindAll() ?? new List<Server>();


            //var ex = Excepted(items);


            //foreach (var srv in ex)
            //{
            //    var first = Servers.First(x => ((Server)x.Tag).Equals(srv));
            //    Servers.Remove(first);
            //}

            Servers.Clear();

            foreach (var srv in items)
            {
                var dd = new MenuItem { Header = srv.Title, Tag = srv };
                dd.Click += srvClicked;
                dd.IsCheckable = true;
                Servers.Add(dd);
            }

        }


        //private bool Contains(Server srv)
        //{
        //    foreach (var item in Servers)
        //    {
        //        if (item is MenuItem mi && mi.Tag is Server serv)
        //        {
        //            if (serv.Equals(srv))
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        //private List<Server> Excepted(IEnumerable<Server> list)
        //{
        //    var items = Servers.Select(x => (Server)x.Tag).ToList();

        //    var itms = items.Except(list).ToList();

        //    return itms;
        //}

        private void srvClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var srv = mi.Tag as Server;

                foreach (var item in Servers)
                {
                    item.IsChecked = false;
                }

                _eventAggregator.PublishOnUIThreadAsync(new Messages.ReconnectRequested { srv = srv });
            }
        }


        private void langChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var ci = mi.Tag as CultureInfo;
                foreach (var item in Languages)
                {
                    item.IsChecked = false;
                }
                LocalizationManager.CurrentCulture = ci;
            }
        }

        #endregion

        #region Config&HotKeys
        private void Init_HK()
        {

            lock (syncObjKeys)
            {
                RegisteredKeys ??= new[]
                {
                    new HotKey(Key.C,KeyModifier.Ctrl, ShowConnectionManager, _view.GetWindowHandle()),
                    new HotKey(Key.N, KeyModifier.Ctrl, Add, _view.GetWindowHandle()),
                    new HotKey(Key.M, KeyModifier.Ctrl, AddMagnet, _view.GetWindowHandle()),
                    new HotKey(Key.X, KeyModifier.Alt, Exit, _view.GetWindowHandle()),
                    new HotKey(Key.F2, KeyModifier.None, Rename, _view.GetWindowHandle()),
                    new HotKey(Key.Escape, KeyModifier.None, Unselect, _view.GetWindowHandle()),
                    new HotKey(Key.F, KeyModifier.Ctrl, Find, _view.GetWindowHandle()),
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
            try
            {
                _view.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                _view.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                _view.CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                //_view.MoreInfoColumn.Height = new GridLength(ConfigService.Instanse.MoreInfoHeight);
                _view.Width = ConfigService.Instanse.MainWindowWidth;
                _view.Height = ConfigService.Instanse.MainWindowHeight;
                _view.WindowState = ConfigService.Instanse.MainWindowState;
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                MessageBoxViewModel.ShowDialog(Kebler.Resources.Strings.ConfigApllyError, manager);
            }
        }

        public void SaveConfig()
        {
            //var size = new StringBuilder();
            //foreach (var column in TorrentsDataGrid.Columns)
            //{
            //    size.Append($"{column.ActualWidth},");
            //}


            ConfigService.Instanse.MainWindowHeight = _view.ActualHeight;
            ConfigService.Instanse.MainWindowWidth = _view.ActualWidth;
            ConfigService.Instanse.MainWindowState = _view.WindowState;
            ConfigService.Instanse.CategoriesWidth = _view.CategoriesColumn.Width.Value;
            ConfigService.Instanse.MoreInfoHeight = _view.MoreInfoColumn.Height.Value;
            //ConfigService.Instanse.ColumnSizes = size.ToString().TrimEnd(',');

            ConfigService.Save();
        }
        #endregion


        #region Events

        public async void TorrentChanged(DataGrid obj, TorrentInfo inf)
        {
            SelectedTorrent = inf;

            try
            {
              


                if (obj != null)
                {
                    SelectedTorrents = obj.SelectedItems.Cast<TorrentInfo>().ToArray();

                    _moreInfoCancelTokeSource?.Cancel();
                    _moreInfoCancelTokeSource = new CancellationTokenSource();

                    await Task.Delay(250, _moreInfoCancelTokeSource.Token);

                    if (_moreInfoCancelTokeSource.Token.IsCancellationRequested)
                        return;


                    UpdateMoreInfoPosition(SelectedTorrents.Any());
                    selectedIDs = SelectedTorrents.Select(x => x.Id).ToArray();
                    await UpdateMoreInfoView(_moreInfoCancelTokeSource.Token);

                    if (selectedIDs.Length == 1)
                        StartUpdateingMoreInfoCycle();
                    else
                    {
                        StoppdateingMoreInfoCycle();
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
        }

        public async Task UpdateMoreInfoView(CancellationToken token)
        {
            if (selectedIDs.Length != 1)
            {
                MoreInfoView.IsMore = true;
                MoreInfoView.Clear();
                MoreInfoView.SelectedCount = selectedIDs.Length;
                return;
            }

            MoreInfoView.IsMore = false;
            MoreInfoView.Loading = true;

            MoreInfoView.id = selectedIDs;
            await PerformMoreInfoUpdate(token);

            MoreInfoView.Loading = false;

        }

        async Task PerformMoreInfoUpdate(CancellationToken token)
        {

            while (true)
            {
                var answ = await _transmissionClient.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS, token, MoreInfoView.id);
                if (token.IsCancellationRequested || Application.Current.Dispatcher.HasShutdownStarted)
                    return;
                if (answ != null)
                {
                    var torrent = answ.Torrents.FirstOrDefault();
                    if (torrent != null)
                    {
                        MoreInfoView.Update(torrent,_transmissionClient);
                    }
                    return;
                }

                await Task.Delay(100, token);
            }

        }


        private void UpdateMoreInfoPosition(bool hide)
        {
            if (hide)
            {
                _view.MoreInfoColumn.MinHeight = 202D;
                _view.MoreInfoColumn.Height = ConfigService.Instanse.MoreInfoHeight >= DefaultSettings.MoreInfoColumnMaxHeight ? new GridLength(DefaultSettings.MoreInfoColumnMaxHeight) : new GridLength(ConfigService.Instanse.MoreInfoHeight);
                _view.MoreInfoColumn.MaxHeight = DefaultSettings.MoreInfoColumnMaxHeight;
            }
            else
            {
                _oldMoreInfoColumnHeight = _view.MoreInfoColumn.ActualHeight;
                _view.MoreInfoColumn.MinHeight = 0;
                _view.MoreInfoColumn.Height = new GridLength(0);
            }
        }

        public void HorDragCompleted()
        {
            _oldMoreInfoColumnHeight = _view.MoreInfoColumn.ActualHeight;
        }
        #endregion


        #region Connection

        public void Retry()
        {

            IsErrorOccuredWhileConnecting = false;
            UpdateServers();
            InitConnection();

        }

        private async Task<string> GetPassword()
        {
            string dd = null;
            await Execute.OnUIThreadAsync(async () =>
            {
                var dialog = new DialogBoxViewModel(Resources.Strings.DialogBox_EnterPWD, string.Empty, true);

                await manager.ShowDialogAsync(dialog);

                dd = dialog.Value;
            });

            if (dd == null)
                throw new TaskCanceledException();

            return dd;

        }

        public void UpdateServers()
        {
            var servers = StorageRepository.GetServersList();
            _servers = servers.FindAll().ToList();
        }

        public void InitConnection()
        {

            if (IsConnected) return;

            UpdateServers();


            if (ServersList.Count > 0)
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
            IsConnectedStatusText = Strings.MW_ConnectingText;
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
                    _transmissionClient = new TransmissionClient(SelectedServer.FullUriPath, null,
                        SelectedServer.UserName,
                        SelectedServer.AskForPassword
                            ? pass
                            : SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password));
                    Log.Info("Client created");
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
            catch (TaskCanceledException)
            {
                IsConnectedStatusText = "Canceled";
            }
            finally
            {
                IsConnecting = false;
            }
        }

        void StartCycle()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;
            //MoreInfoControl.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
            _whileCycleTask = new Task(async () =>
            {
                try
                {
                    var info = await Get(_transmissionClient.GetSessionInformationAsync(_cancelTokenSource.Token), Strings.MW_StatusText_Session);
                    //var info = _transmissionClient.GetSessionInformationAsync(_cancelTokenSource.Token);

                    if (CheckResponse(info.Response))
                    {
                        _sessionInfo = info.Value;
                        IsConnected = true;
                        Log.Info($"Connected {_sessionInfo.Version}");
                        ConnectedServer = SelectedServer;

                        if (App.Instance.torrentsToAdd.Count > 0)
                            OpenPaseedWithArgsFiles();

                        while (IsConnected && !token.IsCancellationRequested)
                        {

                            if (Application.Current?.Dispatcher != null &&
                                Application.Current.Dispatcher.HasShutdownStarted)
                                throw new TaskCanceledException();


                            _stats = (await Get(_transmissionClient.GetSessionStatisticAsync(_cancelTokenSource.Token), Strings.MW_StatusText_Stats)).Value;

                            allTorrents = (await Get(_transmissionClient.TorrentGetAsync(TorrentFields.WORK, _cancelTokenSource.Token), Strings.MW_StatusText_Torrents)).Value;
                            _settings = (await Get(_transmissionClient.GetSessionSettingsAsync(_cancelTokenSource.Token), Strings.MW_StatusText_Settings)).Value;
                            ParseSettings();
                            ParseStats();

                            if (allTorrents.Clone() is TransmissionTorrents data)
                            {
                                ProcessParsingTransmissionResponse(data);
                            }

                            await Task.Delay(ConfigService.Instanse.UpdateTime, token);
                        }
                    }
                    else
                    {
                        IsErrorOccuredWhileConnecting = true;
                        if (info.Response.WebException != null)
                            throw info.Response.WebException;

                        if (info.Response.CustomException != null)
                            throw info.Response.CustomException;

                        throw new Exception(info.Response.HttpWebResponse.StatusCode.ToString());
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

                    ConnectedServer = null;
                    IsConnecting = false;
                    allTorrents = null;
                    TorrentList = new BindableCollection<TorrentInfo>();
                    IsConnected = false;
                    Categories.Clear();
                    IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                    Log.Info("Disconnected from server");
                    if (requested)
                        await _eventAggregator.PublishOnBackgroundThreadAsync(new Messages.ReconnectAllowed(), CancellationToken.None);
                }
            }, token);

            _whileCycleTask.Start();
        }

        public void OpenPaseedWithArgsFiles()
        {
            State = WindowState.Normal;
            if (App.Instance.torrentsToAdd.Count > 0)
            {
                var ch = new Task(() =>
                {
                    Execute.OnUIThread(() =>
                    {
                        OpenTorrent(App.Instance.torrentsToAdd);
                        App.Instance.torrentsToAdd = new List<string>();
                    });

                }, _cancelTokenSource.Token);
                ch.Start();
            }

        }

        private bool CheckResponse(TransmissionResponse resp)
        {

            if (resp.WebException != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = resp.WebException.Status switch
                    {
                        System.Net.WebExceptionStatus.NameResolutionFailure => $"{Kebler.Resources.Strings.EX_Host} '{SelectedServer.FullUriPath}'",
                        _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                    };

                    MessageBoxViewModel.ShowDialog(msg, manager, string.Empty, Enums.MessageBoxDilogButtons.Ok);
                });
                return false;
            }

            else if (resp.CustomException != null)
            {

            }
            return true;
        }

        private async Task<TransmissionResponse<T>> Get<T>(Task<TransmissionResponse<T>> t, string status)
        {
            try
            {
                _isLongTaskRunning = true;
                _longActionTimeStart = DateTimeOffset.Now;
                LongStatusText = status;
                var resp = await t;
                resp.ParseTransmissionReponse(Log);
                return resp;
            }
            finally
            {
                IsDoingStuff = false;
                LongStatusText = string.Empty;
                _isLongTaskRunning = false;
            }
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
                                    $"      {Strings.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                    $"      {Strings.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                    $"      {Strings.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.DownloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.UploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;
            var altUp = _settings.AlternativeSpeedEnabled == true ? $" [{BytesToUserFriendlySpeed.GetSizeString((long)_settings.AlternativeSpeedUp * 1000)}]" : string.Empty;
            var altD = _settings.AlternativeSpeedEnabled == true ? $" [{BytesToUserFriendlySpeed.GetSizeString((long)_settings.AlternativeSpeedDown * 1000)}]" : string.Empty;

            DownloadSpeed = $"D: {dSpeed}{altD}";
            UploadSpeed = $"U: {uSpeed}{altUp}";
        }

        private void ProcessParsingTransmissionResponse(TransmissionTorrents data)
        {
            if (!IsConnected)
                return;


            lock (_syncTorrentList)
            {
                var torrents = new List<TorrentInfo>(TorrentList);
                //1: 'check pending',
                //2: 'checking',

                //5: 'seed pending',
                //6: 'seeding',
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

                if (!string.IsNullOrEmpty(FilterText))
                {
                    //var txtfilter = FilterText;
                    if (FilterText.Contains("{p}:"))
                    {
                        var splited = FilterText.Split("{p}:");
                        var filterKey = splited[^1];
                        // txtfilter = txtfilter.Replace($"{{p}}:{filterKey}", string.Empty);

                        data.Torrents = data.Torrents.Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToArray();
                    }
                    else
                    {
                        data.Torrents = data.Torrents.Where(x => x.Name.ToLower().Contains(FilterText.ToLower())).ToArray();
                    }

                }

                for (var i = 0; i < data.Torrents.Length; i++)
                {
                    data.Torrents[i] = ValidateTorrent(data.Torrents[i]);
                }

                TorrentList = new BindableCollection<TorrentInfo>(data.Torrents);

                UpdateCategories(allTorrents.Torrents.Select(x => new FolderCategory(x.DownloadDir)).ToList());

            }

            //on itemSource update, datagrid lose focus for selected row. 
            #region //♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿
            Execute.OnUIThread(() =>
            {
                var tm = FocusManager.GetFocusedElement(_view);
                if (_view.TorrentsDataGrid.IsFocused)
                {
                    if (FocusManager.GetFocusedElement(_view) is DataGridCell)
                    {
                        var itm = _view.TorrentsDataGrid.SelectedCells.FirstOrDefault();
                        if (itm.Item == null) return;

                        var dd = GetDataGridCell(itm);
                        if (dd != null)
                        {
                            FocusManager.SetFocusedElement(_view, dd);
                        }
                    }

                }

            });
            #endregion //♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿♿

        }

        public DataGridCell GetDataGridCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            return (DataGridCell)cellContent?.Parent;
        }

        public TorrentInfo ValidateTorrent(TorrentInfo torrInf, bool skip = false)
        {
            if (torrInf.Status == 1 || torrInf.Status == 2)
            {
                torrInf.PercentDone = torrInf.RecheckProgress;
                return torrInf;
            }

            if (torrInf.Status == 0)
                return torrInf;


            if (!skip && torrInf.TrackerStats.Length > 0 && torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false))
            {
                torrInf.Status = -1;
            }

            return torrInf;

        }


        private void UpdateCategories(List<FolderCategory> dirrectories)
        {
            if (dirrectories == null) return;
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
                    Application.Current.Dispatcher.Invoke(() => { Categories.Remove(itm); });
                }
            }

            // add torrent counter to title
            Application.Current.Dispatcher.Invoke(() =>
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
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var ct = Categories.Count(x => x.FolderName == itm.FolderName);
                        if (ct <=0)
                        {
                            Categories.Add(itm);
                        }
                        else
                        {
                            foreach (var cat in Categories)
                            {
                                if (cat.FolderName == itm.FolderName)
                                    cat.Title =
                                        $"{cat.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            }
                            itm.Title =
                                $"{itm.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            Categories.Add(itm);
                        }
                        
                    });
                }
            }


        }
        #endregion

        public Task HandleAsync(Messages.LocalizationCultureChangesMessage message, CancellationToken cancellationToken)
        {
            foreach (var item in Languages)
            {
                item.IsChecked = Equals((CultureInfo)item.Tag, message.Culture);
            }

            if (IsConnected)
            {
                IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                        $"      {Strings.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                        $"      {Strings.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                        $"      {Strings.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ReconnectRequested message, CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                requested = true;
                _cancelTokenSource.Cancel();
                IsConnecting = true;
                _SelectedServer = message.srv;
            }
            else
            {
                IsConnecting = true;
                _SelectedServer = message.srv;
                InitConnection();
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ReconnectAllowed message, CancellationToken cancellationToken)
        {
            requested = false;
            InitConnection();
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ConnectedServerChanged message, CancellationToken cancellationToken)
        {
            if (message.srv == null)
            {
                foreach (var item in Servers)
                {
                    item.IsChecked = false;
                }
                return Task.CompletedTask;
            }

            foreach (var item in Servers)
            {
                if (item.Tag is Server srv)
                {
                    if (srv.Equals(message.srv))
                    {
                        item.IsChecked = true;
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ServersUpdated message, CancellationToken cancellationToken)
        {
            ReInitServers();
            return Task.CompletedTask;
        }
    }


    public partial class KeblerViewModel //WindowActins
    {
        public void ActivatedW()
        {
            Init_HK();
        }


        public void Exit()
        {
            TryCloseAsync();
        }

        public void ClosingW()
        {
            SaveConfig();

            Log.Info("-----------Exit-----------");
        }

        public void DeactivatedW()
        {
            UnregisterHotKeys();
        }

        public void CatChange()
        {
            if (!IsConnected)
                return;


            lock (_syncTorrentList)
            {
                _filterCategory = SelectedCat.Cat;
            }


            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessParsingTransmissionResponse(data);
            }

        }

        public void Unselect()
        {
            UpdateMoreInfoPosition(false);
            StoppdateingMoreInfoCycle();

            Execute.OnUIThread(() =>
            {
                if (_view.TorrentsDataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    _view.TorrentsDataGrid.SelectedCells.Clear();

                if (_view.TorrentsDataGrid.SelectionMode != DataGridSelectionMode.Single) //if the Extended mode
                    _view.TorrentsDataGrid.SelectedItems.Clear();
                else
                    _view.TorrentsDataGrid.SelectedItem = null;
            });
        }

        public void ClearFilter()
        {
            FilterText = string.Empty;
            SelectedFolderIndex = -1;
        }

        public void Find()
        {
            Execute.OnUIThread(() =>
            {
                _view.FilterTextBox.Focus();
            });
        }

        public void FilterTextChanged()
        {
            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessParsingTransmissionResponse(data);
            }
        }

        public void ChangeFolderFilter()
        {
            if (SelectedFolderIndex != -1 && SelectedFolder != null)
            {
                FilterText = $"{{p}}:{SelectedFolder.FullPath}";
            }
        }
    }

    public partial class KeblerViewModel //TorrentActions
    {
        #region server
        public async void Slow()
        {
            IsSlowModeEnabled = !IsSlowModeEnabled;
            var resp = await _transmissionClient.SetSessionSettingsAsync(new SessionSettings { AlternativeSpeedEnabled = !_settings.AlternativeSpeedEnabled }, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public void Add()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Torrent files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            OpenTorrent(openFileDialog.FileNames);
        }
        #endregion


        public async void Rename()
        {
            if (selectedIDs.Length > 1)
            {
                await MessageBoxViewModel.ShowDialog(Resources.Strings.MSG_OnlyOneTorrent, manager);
                return;
            }

            if (selectedIDs.Length < 1)
            {
                await MessageBoxViewModel.ShowDialog(Resources.Strings.MSG_SelectOneTorrent, manager);
                return;
            }

            if (!IsConnected) return;

            var sel = SelectedTorrent;
            var dialog = new DialogBoxViewModel(Resources.Strings.MSG_InterNewName, sel.Name, false);

            var result = await manager.ShowDialogAsync(dialog);

            if (result == true)
            {
                var resp = await _transmissionClient.TorrentRenamePathAsync(sel.Id, sel.Name, dialog.Value, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void SetLoc()
        {
            if (!IsConnected) return;

            //var items = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToList();
            var question = selectedIDs.Length > 1 ? Kebler.Resources.Strings.SetLocForMany.Replace("%d", selectedIDs.Length.ToString())
                : Kebler.Resources.Strings.SetLocOnce;

            var path = SelectedTorrent.DownloadDir;

            //var dialog = new DialogBoxView(question, path, false) { Owner = Application.Current.MainWindow };

            var dialog = new DialogBoxViewModel(question, path, false);

            var result = await manager.ShowDialogAsync(dialog);

            if ((bool)result)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var itms = selectedIDs;
                    while (true)
                    {
                        if (Application.Current.Dispatcher.HasShutdownStarted) return;
                        var resp = await _transmissionClient.TorrentSetLocationAsync(itms, dialog.Value, true, _cancelTokenSource.Token);
                        resp.ParseTransmissionReponse(Log);

                        if (CheckResponse(resp))
                            break;

                        await Task.Delay(500, _cancelTokenSource.Token);
                    }
                }, _cancelTokenSource.Token);
            }

            dialog.Value = null;
        }

        public async void Pause()
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void AddMagnet()
        {
            if (!IsConnected) return;

            var dialog = new DialogBoxViewModel(Resources.Strings.MSG_LinkOrMagnet, string.Empty, false);

            var result = await manager.ShowDialogAsync(dialog);

            if (result == true)
            {
                var newTr = new NewTorrent
                {
                    Filename = dialog.Value,
                    Paused = false
                };

                var resp = await _transmissionClient.TorrentAddAsync(newTr, _cancelTokenSource.Token);
            }
        }

        public async void PasueAll()
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void Start()
        {
            if (!IsConnected) return;
            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentStartAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void StartAll()
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStartAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);
        }

        public void Remove()
        {
            RemoveTorrent();
        }

        public void RemoveWithData()
        {
            RemoveTorrent(true);
        }

        public async void Verify()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentVerifyAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void Reannounce()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.ReannounceTorrentsAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void MoveTop()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveTopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void MoveUp()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveUpAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void MoveDown()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveDownAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public async void MoveBot()
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveBottomAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public void Properties(object obj)
        {
            if (obj != null && obj is TorrentInfo tr)
            {
                manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient,new[]{ tr.Id }));
            }
            else
            {
                var asd = SelectedTorrents.Select(x => x.Id).ToArray();
                manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient, asd));
            }

           
        }

        public void CopyMagnet()
        {
            Clipboard.SetText(SelectedTorrent.MagnetLink);
        }



        private void RemoveTorrent(bool removeData = false)
        {
            var toRemove = selectedIDs;
            var dialog = new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), toRemove, ref _transmissionClient, removeData) { Owner = Application.Current.MainWindow };
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
                            ProcessParsingTransmissionResponse(data);
                        }

                    }

                }
            }
        }

        private void OpenTorrent(IEnumerable<string> names)
        {
            foreach (var item in names)
            {
                if (string.IsNullOrEmpty(item))
                    continue;

                if (item.StartsWith("magnet"))
                {
                    var tr = new NewTorrent
                    {
                        Filename = item
                    };
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    _transmissionClient.TorrentAddAsync(tr, _cancelTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    var dialog = new AddTorrentView(item, _transmissionClient, Application.Current.MainWindow,_settings);

                    if (dialog.ShowDialog() == false)
                        return;

                    if (dialog.TorrentResult.Value.Status != Enums.AddTorrentStatus.Added) continue;

                    TorrentList.Add(new TorrentInfo(dialog.TorrentResult.Value.ID)
                    {
                        Name = dialog.TorrentResult.Value.Name,
                        HashString = dialog.TorrentResult.Value.HashString
                    });
                }
            }
        }


    }


    public partial class KeblerViewModel
    {
        readonly IWindowManager manager = new WindowManager();
        private bool requested = false;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private BindableCollection<StatusCategory> _categoriesList = new BindableCollection<StatusCategory>();
        private BindableCollection<TorrentInfo> _torrentList = new BindableCollection<TorrentInfo>();
        private BindableCollection<FolderCategory> _folderCategory = new BindableCollection<FolderCategory>();

        private StatusCategory _selectedCat;
        private int _selectedCategoryIndex = -1;
        private List<Server> _servers;
        private BindableCollection<MenuItem> servers = new BindableCollection<MenuItem>();
        private Task _checkerTask;
        private DateTimeOffset _longActionTimeStart;
        private bool _isLongTaskRunning;
        public TransmissionClient _transmissionClient;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _moreInfoCancelTokeSource = new CancellationTokenSource();
        private Task _whileCycleTask;
        private Task _whileCycleMoreInfoTask;
        private SessionInfo _sessionInfo;
        private Statistic _stats;
        private TransmissionTorrents allTorrents = new TransmissionTorrents();
        public SessionSettings _settings;
        public bool _isConnecting, _isDoingStuff, _isConnected, _isErrorOccuredWhileConnecting, _isSlowModeEnabled;
        private object _syncTorrentList = new object();
        private Enums.Categories _filterCategory;
        private string _isConnectedStatusText, _longStatusText, _downloadSpeed, _uploadSpeed, _filterText;
        private Server _SelectedServer, _ConnectedServer;
        private TorrentInfo[] SelectedTorrents = new TorrentInfo[0];
        private TorrentInfo _selectedTorrent;
        private uint[] selectedIDs;
        private HotKey[] RegisteredKeys;
        private object syncObjKeys = new object();
        private KeblerView _view;
        private double _MoreInfoColumnHeight, _oldMoreInfoColumnHeight, _minMoreInfoColumnHeight;
        private int _selectedFolderIndex;
        private FolderCategory _selectedFolder;
        private WindowState _state;
        private List<Server> ServersList
        {
            get
            {
                if (_servers == null)
                {
                    UpdateServers();
                }
                return _servers;
            }
        }


        public MoreInfoViewModel MoreInfoView { get; set; } = new MoreInfoViewModel();

        public BindableCollection<FolderCategory> Categories
        {
            get => _folderCategory;
            set => Set(ref _folderCategory, value);
        }


        private BindableCollection<MenuItem> _languages = new BindableCollection<MenuItem>();
        public BindableCollection<MenuItem> Languages
        {
            get => _languages;
            set => Set(ref _languages, value);
        }

        public BindableCollection<MenuItem> Servers
        {
            get => servers;
            set => Set(ref servers, value);
        }

        public TorrentInfo SelectedTorrent
        {
            get => _selectedTorrent;
            set => Set(ref _selectedTorrent, value);
        }

        public BindableCollection<StatusCategory> CategoriesList
        {
            get => _categoriesList;
            set => Set(ref _categoriesList, value);
        }

        public BindableCollection<TorrentInfo> TorrentList
        {
            get => _torrentList;
            set => Set(ref _torrentList, value);
        }

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set => Set(ref _selectedCategoryIndex, value);
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            set => Set(ref _isConnecting, value);
        }

        public string IsConnectedStatusText
        {
            get => _isConnectedStatusText;
            set => Set(ref _isConnectedStatusText, value);
        }


        public bool IsDoingStuff
        {
            get => _isDoingStuff;
            set => Set(ref _isDoingStuff, value);
        }
        public bool IsConnected
        {
            get => _isConnected;
            set => Set(ref _isConnected, value);
        }
        public Server SelectedServer
        {
            get => _SelectedServer;
            set => Set(ref _SelectedServer, value);
        }
        public bool IsErrorOccuredWhileConnecting
        {
            get => _isErrorOccuredWhileConnecting;
            set => Set(ref _isErrorOccuredWhileConnecting, value);
        }
        public Server ConnectedServer
        {
            get => _ConnectedServer;
            set
            {
                Set(ref _ConnectedServer, value);
                _eventAggregator.PublishOnUIThreadAsync(new Messages.ConnectedServerChanged { srv = value });
            }
        }
        public string LongStatusText
        {
            get => _longStatusText;
            set => Set(ref _longStatusText, value);
        }
        public bool IsSlowModeEnabled
        {
            get => _isSlowModeEnabled;
            set => Set(ref _isSlowModeEnabled, value);
        }
        public string DownloadSpeed
        {
            get => _downloadSpeed;
            set => Set(ref _downloadSpeed, value);
        }
        public string UploadSpeed
        {
            get => _uploadSpeed;
            set => Set(ref _uploadSpeed, value);
        }
        public string FilterText
        {
            get => _filterText;
            set => Set(ref _filterText, value);
        }
        public int SelectedFolderIndex
        {
            get => _selectedFolderIndex;
            set => Set(ref _selectedFolderIndex, value);
        }

        public StatusCategory SelectedCat
        {
            get => _selectedCat;
            set => Set(ref _selectedCat, value);
        }

        public FolderCategory SelectedFolder
        {
            get => _selectedFolder;
            set => Set(ref _selectedFolder, value);
        }

        public double MoreInfoColumnHeight
        {
            get => _MoreInfoColumnHeight;
            set => Set(ref _MoreInfoColumnHeight, value);
        }
        public double MoreInfoColumnHeightMin
        {
            get => _minMoreInfoColumnHeight;
            set => Set(ref _minMoreInfoColumnHeight, value);
        }

        public WindowState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public string HeaderTitle
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#elif PORTABLE
                return "Kebler [Portable]";
#else
                var assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion}";
#endif
            }
        }
    }
}
