using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using Kebler.Const;
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
using Microsoft.AppCenter.Crashes;
using static Kebler.Models.Messages;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using ILog = log4net.ILog;
using ListView = System.Windows.Controls.ListView;
using LogManager = log4net.LogManager;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;
using TorrentFields = Kebler.Models.Torrent.TorrentFields;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : BaseScreen,
        IHandle<Messages.LocalizationCultureChangesMessage>,
        IHandle<Messages.ReconnectRequested>,
        IHandle<Messages.ReconnectAllowed>,
        IHandle<Messages.ConnectedServerChanged>,
        IHandle<Messages.ServersUpdated>,
        IHandle<Messages.ShowMoreInfoChanged>


    {
        private readonly IEventAggregator? _eventAggregator;

        public KeblerViewModel()
        {
        }


        public KeblerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);
            InitMenu();

            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_AllTorrents)),
                Cat = Enums.Categories.All
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Downloading)),
                Cat = Enums.Categories.Downloading
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Active)),
                Cat = Enums.Categories.Active
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_InActive)),
                Cat = Enums.Categories.Inactive
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Ended)),
                Cat = Enums.Categories.Ended
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Stopped)),
                Cat = Enums.Categories.Stopped
            });
            CategoriesList.Add(new StatusCategory
            {
                Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Error)),
                Cat = Enums.Categories.Error
            });

            SelectedFolderIndex = -1;

            SelectedCat = CategoriesList.First();


            App.Instance.KeblerVM = this;
        }

        public Task HandleAsync(Messages.ConnectedServerChanged message, CancellationToken cancellationToken)
        {
            if (message.srv == null)
            {
                foreach (var item in Servers) item.IsChecked = false;
                return Task.CompletedTask;
            }

            foreach (var item in Servers)
                if (item.Tag is Server srv)
                    if (srv.Equals(message.srv))
                    {
                        item.IsChecked = true;
                        break;
                    }

            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.LocalizationCultureChangesMessage message, CancellationToken cancellationToken)
        {
            _isChangingLang = true;

            try
            {

                foreach (var item in Languages) item.IsChecked = Equals((CultureInfo)item.Tag, message.Culture);

                if (IsConnected && _stats != null)
                    IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_Uploaded))} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_Downloaded))}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                var ind = SelectedCategoryIndex;
                CategoriesList.Clear();

                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_AllTorrents)),
                    Cat = Enums.Categories.All
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Downloading)),
                    Cat = Enums.Categories.Downloading
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Active)),
                    Cat = Enums.Categories.Active
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_InActive)),
                    Cat = Enums.Categories.Inactive
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Ended)),
                    Cat = Enums.Categories.Ended
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Stopped)),
                    Cat = Enums.Categories.Stopped
                });
                CategoriesList.Add(new StatusCategory
                {
                    Title = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Cat_Error)),
                    Cat = Enums.Categories.Error
                });
                SelectedCategoryIndex = ind;
            }
            finally
            {
                _isChangingLang = false;
            }



            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ReconnectAllowed message, CancellationToken cancellationToken)
        {
            requested = false;
            InitConnection();
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

        public Task HandleAsync(Messages.ServersUpdated message, CancellationToken cancellationToken)
        {
            ReInitServers();
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ShowMoreInfoChanged message, CancellationToken cancellationToken)
        {
            IsShowMoreInfo = ConfigService.Instanse.MoreInfoShow;
            MoreInfoView.IsShowMoreInfoCheckNotify = !IsShowMoreInfo;
            return Task.CompletedTask;
        }

        protected override void OnViewAttached(object view, object context)
        {
            _view = view as KeblerView;
            if (_view != null)
            {
                //_view.MoreView.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
                MoreInfoView = new MoreInfoViewModel(_view, UpdateMoreInfoPosition, _eventAggregator);
                _view.MoreView.FileTreeViewControl.Checked += MoreInfoView.MakeRecheck;
            }

            base.OnViewAttached(view, context);
            ApplyConfig();
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(InitConnection, cancellationToken);

            return base.OnActivateAsync(cancellationToken);
        }


        public void ShowConnectionManager()
        {
            var sw = new Stopwatch();
            sw.Start();
            if (_eventAggregator != null)
                manager.ShowDialogAsync(new ConnectionManagerViewModel(_eventAggregator));
            else
            {
                sw.Stop();
                Log.Error(sw, $"{nameof(KeblerViewModel)}.{nameof(ShowConnectionManager)}() => {nameof(_eventAggregator)} is null");
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
                        if (Application.Current != null && Application.Current.Dispatcher.HasShutdownStarted)
                            throw new TaskCanceledException("Dispatcher.HasShutdownStarted  = true");

                        var date = DateTimeOffset.Now;
                        var time = date - _longActionTimeStart;
                        if (time.TotalSeconds > 2 && _isLongTaskRunning) IsDoingStuff = true;
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


        #region TopBarMenu

        private void InitMenu()
        {
            foreach (var item in LocalizationManager.CultureList)
            {
                var name = item.NativeName.ToCharArray();
                if (name.Length > 0)
                {
                    name[0] = Char.ToUpper(name[0]);
                }


                var menuItem = new MenuItem { Header = new string(name), Tag = item };
                menuItem.Click += langChanged;
                menuItem.IsCheckable = true;
                menuItem.IsChecked = Equals(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName,
                    item.TwoLetterISOLanguageName);
                Languages.Add(menuItem);
            }

            ReInitServers();
        }


        public void Settings()
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {ConstStrings.CONFIGPATH}") { CreateNoWindow = true });
        }

        public void Preferences()
        {
            if (_settings != null)
                manager.ShowDialogAsync(new ServerPreferencesViewModel(_settings));
            else
            {
                Log.Error($"{nameof(KeblerViewModel)}.{nameof(Preferences)}() => {nameof(_settings)} is null");
            }
        }

        private void ReInitServers()
        {
            var _dbServersList = StorageRepository.GetServersList();

            var items = _dbServersList?.FindAll() ?? new List<Server>();

            Servers.Clear();

            foreach (var srv in items)
            {
                var dd = new MenuItem { Header = srv.Title, Tag = srv };
                dd.Click += srvClicked;
                dd.IsCheckable = true;
                Servers.Add(dd);
            }
        }

        private void srvClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var srv = mi.Tag as Server;

                foreach (var item in Servers) item.IsChecked = false;

                _eventAggregator.PublishOnUIThreadAsync(new Messages.ReconnectRequested { srv = srv });
            }
        }


        private void langChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var ci = mi.Tag as CultureInfo;
                foreach (var item in Languages) item.IsChecked = false;
                LocalizationManager.CurrentCulture = ci;
            }
        }

        #endregion

        #region Config

        private void ApplyConfig()
        {
            try
            {
                if (_view != null)
                {
                    _view.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    _view.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                    //_view.CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                    _view.Width = ConfigService.Instanse.MainWindowWidth;
                    _view.Height = ConfigService.Instanse.MainWindowHeight;
                    _view.WindowState = ConfigService.Instanse.MainWindowState;
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                    IsShowMoreInfo = ConfigService.Instanse.MoreInfoShow;
                }
                else
                {
                    throw new Exception($" Filed '{nameof(_view)}' is null");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Crashes.TrackError(ex);

                MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.ConfigApllyError)), manager);
            }
        }

        public void SaveConfig()
        {
            if (_view != null)
            {
                ConfigService.Instanse.MainWindowHeight = _view.ActualHeight;
                ConfigService.Instanse.MainWindowWidth = _view.ActualWidth;
                ConfigService.Instanse.MainWindowState = _view.WindowState;
                ConfigService.Instanse.CategoriesWidth = _view.CategoriesColumn.Width.Value;
                ConfigService.Instanse.MoreInfoHeight = _view.MoreInfoColumn.Height.Value;
                ConfigService.Instanse.MoreInfoShow = IsShowMoreInfo;
                ConfigService.Save();
            }
        }

        #endregion


        #region Events

        public async void TorrentChanged(ListView obj, TorrentInfo inf)
        {
            try
            {
                SelectedTorrents = obj.SelectedItems.Cast<TorrentInfo>().ToArray();

                _moreInfoCancelTokeSource?.Cancel();
                _moreInfoCancelTokeSource = new CancellationTokenSource();

                await Task.Delay(250, _moreInfoCancelTokeSource.Token);

                if (_moreInfoCancelTokeSource.Token.IsCancellationRequested)
                    return;
                selectedIDs = SelectedTorrents.Select(x => x.Id).ToArray();

                if (ConfigService.Instanse.MoreInfoShow)
                {
                    UpdateMoreInfoPosition(SelectedTorrents.Any());
                    UpdateMoreInfoView(_moreInfoCancelTokeSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public void UpdateMoreInfoView(CancellationToken token)
        {
            if (selectedIDs.Length != 1)
            {
                MoreInfoView.IsMore = true;
                MoreInfoView.SelectedCount = selectedIDs.Length;
                return;
            }

            MoreInfoView.IsMore = false;
            MoreInfoView.Update(selectedIDs, _transmissionClient, token);
        }


        private void UpdateMoreInfoPosition(bool hide)
        {
            if (_view != null)
            {
                if (hide)
                {
                    _view.MoreInfoColumn.MinHeight = 300D;
                    _view.MoreInfoColumn.Height = new GridLength(_oldMoreInfoColumnHeight);
                    //ConfigService.Instanse.MoreInfoHeight >= DefaultSettings.MoreInfoColumnMaxHeight
                    //    ? new GridLength(DefaultSettings.MoreInfoColumnMaxHeight)
                    //    : new GridLength(ConfigService.Instanse.MoreInfoHeight);
                    //_view.MoreInfoColumn.MaxHeight = DefaultSettings.MoreInfoColumnMaxHeight;
                }
                else
                {
                    _oldMoreInfoColumnHeight = _view.MoreInfoColumn.ActualHeight;
                    _view.MoreInfoColumn.MinHeight = 0;
                    _view.MoreInfoColumn.Height = new GridLength(0);
                }
            }
        }

        public void HorDragCompleted()
        {
            if (_view != null)
                _oldMoreInfoColumnHeight = _view.MoreInfoColumn.ActualHeight;
        }

        #endregion


        #region Connection

        public void Retry()
        {
            IsErrorOccuredWhileConnecting = false;
            _servers = GetServers();
            InitConnection();
        }

        private async Task<string> GetPassword()
        {
            string? passwordResult = null;
            await Execute.OnUIThreadAsync(async () =>
            {
                var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.DialogBox_EnterPWD)), string.Empty, true);

                await manager.ShowDialogAsync(dialog);

                passwordResult = dialog.Value.ToString();
            });

            if (passwordResult == null)
                throw new TaskCanceledException();

            return passwordResult;
        }

        private List<Server> GetServers()
        {
            var bdServers = StorageRepository.GetServersList();
            return bdServers.FindAll().ToList();
        }

        private void InitConnection()
        {
            if (IsConnected) return;

            _servers = GetServers();


            if (ServersList.Count > 0)
            {
                if (_checkerTask == null)
                {
                    _checkerTask = GetLongChecker();
                    _checkerTask.Start();
                }

                var srv = ServersList.FirstOrDefault();
                if (srv is not null)
                {
                    SelectedServer = srv;

                    TryConnect();
                }

            }
        }

        private async void TryConnect()
        {
            Log.Info("Try initialize connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_ConnectingText));
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
                    Crashes.TrackError(ex);

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

        private void StartCycle()
        {
            _cancelTokenSource = new CancellationTokenSource();
            var token = _cancelTokenSource.Token;
            //MoreInfoControl.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
            _whileCycleTask = new Task(async () =>
            {
                if (_transmissionClient != null)
                {
                    try
                    {
                        var info = await Get(_transmissionClient.GetSessionInformationAsync(_cancelTokenSource.Token),
                            LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_StatusText_Session)));
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


                                _stats = (await Get(
                                    _transmissionClient.GetSessionStatisticAsync(_cancelTokenSource.Token),
                                    LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_StatusText_Stats)))).Value;

                                allTorrents =
                                    (await Get(
                                        _transmissionClient.TorrentGetAsync(TorrentFields.WORK,
                                            _cancelTokenSource.Token),
                                        LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_StatusText_Torrents)))).Value;
                                _settings = (await Get(
                                    _transmissionClient.GetSessionSettingsAsync(_cancelTokenSource.Token),
                                    LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_StatusText_Settings)))).Value;
                                ParseSettings();
                                ParseStats();

                                if (allTorrents.Clone() is TransmissionTorrents data)
                                    ProcessParsingTransmissionResponse(data);

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

                            if (info.Response.HttpWebResponse != null)
                                throw new Exception(info.Response.HttpWebResponse.StatusCode.ToString());
                        }
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                        Crashes.TrackError(ex);

                    }
                    finally
                    {
                        ConnectedServer = null;
                        IsConnecting = false;
                        TorrentList = new Bind<TorrentInfo>();
                        IsConnected = false;
                        Categories?.Clear();
                        IsConnectedStatusText = DownloadSpeed = UploadSpeed = string.Empty;
                        Log.Info("Disconnected from server");
                        if (requested)
                            await _eventAggregator.PublishOnBackgroundThreadAsync(new Messages.ReconnectAllowed(),
                                CancellationToken.None);
                    }
                }
            }, token);

            _whileCycleTask.Start();
        }

        public void OpenPaseedWithArgsFiles()
        {
            if (App.Instance.torrentsToAdd.Count > 0)
            {
                var ch = new Task(() =>
                {
                    Execute.OnUIThread(async () =>
                    {
                        State = WindowState.Normal;
                        Application.Current.MainWindow?.Activate();
                        await OpenTorrent(App.Instance.torrentsToAdd);
                        App.Instance.torrentsToAdd.Clear();
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
                        WebExceptionStatus.NameResolutionFailure =>
                            $"{LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.EX_Host))} '{SelectedServer.FullUriPath}'",
                        _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                    };

                    MessageBoxViewModel.ShowDialog(msg, manager, string.Empty);
                });
                return false;
            }

            if (resp.CustomException != null)
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
                                    $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_Uploaded))} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                    $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_Downloaded))}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                    $"      {LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.DownloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(_stats.UploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;
            var altUp = _settings?.AlternativeSpeedEnabled == true
                ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedUp * 1000)}]"
                : string.Empty;
            var altD = _settings?.AlternativeSpeedEnabled == true
                ? $" [{BytesToUserFriendlySpeed.GetSizeString(_settings.AlternativeSpeedDown * 1000)}]"
                : string.Empty;

            DownloadSpeed = $"D: {dSpeed}{altD}";
            UploadSpeed = $"U: {uSpeed}{altUp}";
        }

        private void ProcessParsingTransmissionResponse(TransmissionTorrents data)
        {
            if (!IsConnected)
                return;

            lock (_syncTorrentList)
            {
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
                        data.Torrents = data.Torrents.Where(x => x.Status == 4 || x.Status == 6 || x.Status == 2)
                            .ToArray();
                        data.Torrents = data.Torrents.Where(x => x.RateDownload > 1 || x.RateUpload > 1).ToArray();
                        break;

                    //0: 'stopped' and is error,
                    case Enums.Categories.Stopped:
                        data.Torrents = data.Torrents.Where(x => x.Status == 0 && string.IsNullOrEmpty(x.ErrorString))
                            .ToArray();
                        break;

                    case Enums.Categories.Error:
                        data.Torrents = data.Torrents.Where(x => !string.IsNullOrEmpty(x.ErrorString)).ToArray();
                        break;


                    //6: 'seeding',
                    //1: 'checking queue',
                    case Enums.Categories.Inactive:
                        //var array1 = data.Torrents.Where(x=> x.Status == 1).ToArray();
                        var array2 = data.Torrents.Where(x => x.RateDownload <= 0 && x.RateUpload <= 0 && x.Status != 2)
                            .ToArray();

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

                        data.Torrents = data.Torrents
                            .Where(x => FolderCategory.NormalizePath(x.DownloadDir).Equals(filterKey)).ToArray();
                    }
                    else
                    {
                        data.Torrents = data.Torrents.Where(x => x.Name.ToLower().Contains(FilterText.ToLower()))
                            .ToArray();
                    }
                }

                for (var i = 0; i < data.Torrents.Length; i++) data.Torrents[i] = ValidateTorrent(data.Torrents[i]);

                //Debug.WriteLine("S" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateOnUi(data.Torrents);

                //Debug.WriteLine("E" + DateTime.Now.ToString("HH:mm:ss:ffff"));

                UpdateCategories(allTorrents.Torrents.Select(x => new FolderCategory(x.DownloadDir)).ToList());
            }
        }

        private void UpdateOnUi(TorrentInfo[] info)
        {
            var toRm = new List<TorrentInfo>(TorrentList.Except(info).ToList());


            if (toRm.Any())
                OnUIThread(() =>
                {
                    foreach (var item in toRm)
                    {
                        TorrentList.RemoveWithoutNotify(item);
                    }
                });

            foreach (var item in info)
            {
                var ind = TorrentList.IndexOf(item);
                if (ind == -1)
                {
                    TorrentList.Add(item);
                }
                else
                {
                    TorrentList[ind].Notify(item);
                }
            }
        }


        public TorrentInfo ValidateTorrent(TorrentInfo torrInf, bool skip = false)
        {
            // if (torrInf.Status == 1 || torrInf.Status == 2)
            // {
            //     torrInf.PercentDone = torrInf.RecheckProgress;
            //     return torrInf;
            // }
            //
            // if (torrInf.Status == 0)
            //     return torrInf;
            //
            //
            // if (!skip && torrInf.TrackerStats.Length > 0 &&
            //     torrInf.TrackerStats.All(x => x.LastAnnounceSucceeded == false)) torrInf.Status = -1;

            return torrInf;
        }


        private void UpdateCategories(IEnumerable<FolderCategory> dirrectories)
        {
            var dirs = dirrectories.Distinct().ToList();

            var cats = new List<FolderCategory>();
            var toUpd = new List<FolderCategory>();

            foreach (var cat in dirs)
            {
                cat.Count = allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == cat.FullPath);
                cat.Title = cat.FolderName;
                cats.Add(cat);

                if (Categories.Any(x => x.Equals(cat) && cat.Count != x.Count))
                {
                    toUpd.Add(cat);
                }

            }


            var toRm = Categories.Except(cats).ToList();
            var toAdd = cats.Except(Categories).ToList();

            if (toRm.Any())
            {
                Log.Info("Remove categories" + string.Join(", ", toRm));

                foreach (var itm in toRm) Application.Current.Dispatcher.Invoke(() => { Categories.Remove(itm); });
            }

            //update counter for existing
            foreach (var itm in toUpd)
            {
                Categories.First(x => x.Equals(itm)).Count = itm.Count;
            }

            if (toAdd.Any())
            {
                Log.Info("Add categories" + string.Join(", ", toAdd));
                foreach (var itm in toAdd)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var ct = Categories.Count(x => x.FolderName == itm.FolderName);
                        if (ct <= 0)
                        {
                            Categories.Add(itm);
                        }
                        else
                        {
                            foreach (var cat in Categories)
                                if (cat.FolderName == itm.FolderName)
                                    cat.Title =
                                        $"{cat.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            itm.Title =
                                $"{itm.FullPath} ({allTorrents.Torrents.Count(x => FolderCategory.NormalizePath(x.DownloadDir) == itm.FullPath)})";
                            Categories.Add(itm);
                        }
                    });
            }


            if (_isAddWindOpened)
            {
                _eventAggregator.PublishOnUIThreadAsync(new DownlaodCategoriesChanged(Categories));
            }
        }

        #endregion
    }


    public partial class KeblerViewModel //WindowActins
    {
        public void Exit()
        {
            TryCloseAsync();
        }

        public void ClosingW()
        {
            SaveConfig();

            Log.Info("-----------Exit-----------");
        }


        public void CatChange()
        {
            if (!IsConnected)
                return;

            if (!_isChangingLang)
            {
                lock (_syncTorrentList)
                {
                    _filterCategory = SelectedCat.Cat;
                }

            }




            if (allTorrents.Clone() is TransmissionTorrents data)
                ProcessParsingTransmissionResponse(data);
        }


        public void Unselect()
        {
            UpdateMoreInfoPosition(false);
        }

        public void ClearFilter()
        {
            FilterText = string.Empty;
            SelectedFolderIndex = -1;
        }

        public void Find()
        {
            Execute.OnUIThread(() => { _view.FilterTextBox.Focus(); });
        }

        public void FilterTextChanged()
        {
            if (allTorrents.Clone() is TransmissionTorrents data) ProcessParsingTransmissionResponse(data);
        }

        public void ChangeFolderFilter()
        {
            if (SelectedFolderIndex != -1) FilterText = $"{{p}}:{SelectedFolder.FullPath}";
        }
    }

    public partial class KeblerViewModel //TorrentActions
    {
        public async void Rename()
        {
            if (selectedIDs.Length > 1)
            {
                await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MSG_OnlyOneTorrent)), manager);
                return;
            }

            if (selectedIDs.Length < 1)
            {
                await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MSG_SelectOneTorrent)), manager);
                return;
            }

            if (IsConnected && SelectedTorrent != null)
            {
                var sel = SelectedTorrent;
                var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MSG_InterNewName)), sel.Name, false);

                if (await manager.ShowDialogAsync(dialog) == true && _transmissionClient != null)
                {
                    var resp = await _transmissionClient.TorrentRenamePathAsync(sel.Id, sel.Name, dialog.Value.ToString(),
                        _cancelTokenSource.Token);
                    resp.ParseTransmissionReponse(Log);
                }
            }
        }

        public async void SetLoc()
        {
            if (IsConnected && SelectedTorrent != null)
            {
                var question = SelectedTorrents.Length > 1
                    ? LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.SetLocForMany)).Replace("%d", selectedIDs.Length.ToString())
                    : LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.SetLocOnce));

                var path = SelectedTorrent.DownloadDir;
                var dialog = new DialogBoxViewModel(question, Categories.Select(x => x.FullPath), path);

                if (await manager.ShowDialogAsync(dialog) == true && _transmissionClient != null)
                    await Task.Factory.StartNew(async () =>
                    {
                        var itms = SelectedTorrents.Select(x => x.Id);
                        while (true)
                        {
                            if (Application.Current.Dispatcher.HasShutdownStarted) return;
                            var resp = await _transmissionClient.TorrentSetLocationAsync(itms, dialog.Value.ToString(), true,
                                _cancelTokenSource.Token);
                            resp.ParseTransmissionReponse(Log);

                            if (CheckResponse(resp))
                                break;

                            await Task.Delay(500, _cancelTokenSource.Token);
                        }
                    }, _cancelTokenSource.Token);
            }
        }

        public void Pause() => Pause(null);

        public async void Pause(uint[]? ids)
        {
            if (IsConnected && _transmissionClient != null)
            {
                var id = ids ?? selectedIDs;
                var resp = await _transmissionClient.TorrentStopAsync(id, _cancelTokenSource.Token);
                if (resp.Success)
                {
                    foreach (var item in id)
                    {
                        TorrentList.First(x => x.Id == item).Status = 0;
                    }
                }

                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void AddMagnet()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var dialog = new DialogBoxViewModel(Strings.MSG_LinkOrMagnet, string.Empty, false, Enums.MessageBoxDilogButtons.OkCancel, LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.Error_EmptyString)));

                var result = await manager.ShowDialogAsync(dialog);

                if (result == true && dialog.Value is string str && !string.IsNullOrEmpty(str))
                {
                    var newTr = new NewTorrent
                    {
                        Filename = str,
                        Paused = false
                    };

                    await _transmissionClient.TorrentAddAsync(newTr, _cancelTokenSource.Token);
                }
            }
        }

        public async void PauseAll()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var ids = _torrentList.Select(x => x.Id).ToArray();

                var resp = await _transmissionClient.TorrentStopAsync(ids, _cancelTokenSource.Token);
                if (resp.Success)
                {
                    foreach (var item in ids)
                    {
                        var tor = TorrentList.FirstOrDefault(x => x.Id == item);
                        if (tor != null)
                            tor.Status = 0;
                    }
                }

                resp.ParseTransmissionReponse(Log);
            }
        }

        public void Start() => Start(null);

        public async void Start(uint[]? ids)
        {
            if (IsConnected && _transmissionClient != null)
            {
                var id = ids ?? selectedIDs;
                //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
                var resp = await _transmissionClient.TorrentStartAsync(id, _cancelTokenSource.Token);
                if (resp.Success)
                {
                    foreach (var item in id)
                    {
                        var tor = TorrentList.FirstOrDefault(x => x.Id == item);
                        if (tor != null)
                        {
                            tor.Status = tor.PercentDone == 1D ? 6 : 4;
                        }
                    }
                }

                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void StartAll()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var ids = _torrentList.Select(x => x.Id).ToArray();
                var resp = await _transmissionClient.TorrentStartAsync(ids, _cancelTokenSource.Token);
                if (resp.Success)
                {
                    foreach (var item in ids)
                    {
                        var tor = TorrentList.FirstOrDefault(x => x.Id == item);
                        if (tor != null)
                        {
                            tor.Status = tor.PercentDone == 1 ? 6 : 4;
                        }
                    }
                }

                resp.ParseTransmissionReponse(Log);
            }
        }

        public void Remove() => RemoveTorrent();
        public void RemoveWithData() => RemoveTorrent(true);

        public void Space(object obj)
        {
            if (IsConnected && _transmissionClient != null && obj is TorrentInfo tr)
            {
                var status = tr.Status;
                Debug.WriteLine($"Space + {tr.Id}");
                switch (status)
                {
                    case 4:
                    case 6:
                        Pause(new[] { tr.Id });
                        break;
                    default:
                        Start(new[] { tr.Id });
                        break;
                }
            }
        }


        public async void Verify()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.TorrentVerifyAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void Reannounce()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.ReannounceTorrentsAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void MoveTop()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.TorrentQueueMoveTopAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void MoveUp()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.TorrentQueueMoveUpAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void MoveDown()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.TorrentQueueMoveDownAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public async void MoveBot()
        {
            if (IsConnected && _transmissionClient != null)
            {
                var resp = await _transmissionClient.TorrentQueueMoveBottomAsync(selectedIDs, _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        public void Properties()
        {
            if (_transmissionClient != null)
            {
                if (selectedIDs.Length > 1)
                {
                    manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient,
                        SelectedTorrents.Select(x => x.Id).ToArray(), manager));
                }
                else
                {
                    manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient, new[] { SelectedTorrents.Select(x => x.Id).First() }, manager));
                }
            }
        }

        public void CopyMagnet()
        {
            if (SelectedTorrent != null)
                Clipboard.SetText(SelectedTorrent.MagnetLink);
            else
                MessageBoxViewModel.ShowDialog("Please select torrent");
        }

        private async void RemoveTorrent(uint id)
        {
            var toRemove = new[] { id };

            var result = await _transmissionClient?.TorrentRemoveAsync(toRemove, new CancellationToken(), false);


            foreach (var rm in toRemove)
            {
                var itm = TorrentList.First(x => x.Id == rm);
                TorrentList.Remove(itm);

                allTorrents.Torrents = allTorrents.Torrents.Where(val => val.Id == rm).ToArray();
            }


        }

        private void RemoveTorrent(bool removeData = false)
        {
            if (selectedIDs.Length > 0)
            {
                var toRemove = selectedIDs;
                var dialog =
                    new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), toRemove,
                            ref _transmissionClient, removeData)
                    { Owner = Application.Current.MainWindow };

                if (dialog.ShowDialog() == true && dialog.Result == Enums.RemoveResult.Ok)
                    lock (_syncTorrentList)
                    {
                        foreach (var rm in toRemove)
                        {
                            var itm = TorrentList.First(x => x.Id == rm);
                            TorrentList.Remove(itm);

                            allTorrents.Torrents = allTorrents.Torrents.Where(val => val.Id == rm).ToArray();
                        }

                        if (allTorrents.Clone() is TransmissionTorrents data)
                            ProcessParsingTransmissionResponse(data);
                    }
            }
        }

        private async Task OpenTorrent(IEnumerable<string> names)
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
                    await _transmissionClient?.TorrentAddAsync(tr, _cancelTokenSource.Token)!;
                }
                else
                {

                    var downs = _torrentList.Select(c => new { c.Name, c.Id }).Select(c => (c.Name, c.Id));
                    var dialog = new AddTorrentViewModel(item, _transmissionClient, _settings,
                        _torrentList, _folderCategory, _eventAggregator,
                        downs, RemoveTorrent,
                        ref _isAddWindOpened, ref _view);


                    await manager.ShowDialogAsync(dialog);


                    _isAddWindOpened = false;

                    if (dialog.Result == true)
                    {
                        if (dialog.TorrentResult.Value.Status == Enums.AddTorrentStatus.Added)
                            TorrentList.Add(new TorrentInfo(dialog.TorrentResult.Value.ID)
                            {
                                Name = dialog.TorrentResult.Value.Name,
                                HashString = dialog.TorrentResult.Value.HashString
                            });
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        #region server

        public async void Slow()
        {
            if (_transmissionClient != null)
            {
                IsSlowModeEnabled = !IsSlowModeEnabled;

                var resp = await _transmissionClient.SetSessionSettingsAsync(
                    new SessionSettings
                    {
                        AlternativeSpeedEnabled = !_settings?.AlternativeSpeedEnabled
                    },
                    _cancelTokenSource.Token);
                resp.ParseTransmissionReponse(Log);
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public async void Add()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Torrent files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            await OpenTorrent(openFileDialog.FileNames);
        }

        #endregion

        #region Tools

        public async void ClearTrackers()
        {
            if (await MessageBoxViewModel.ShowDialog(
                "All trackers will be removed for each torrent file. Are you sure?", manager, "Confirm action",
                Enums.MessageBoxDilogButtons.YesNo) == true)
            {
                var ids = _torrentList.Select(x => x.Id).ToArray();

                var trackersId = new List<uint>();

                foreach (var tr in _torrentList)
                {
                    foreach (var tracker in tr.TrackerStats)
                    {
                        if (!trackersId.Contains(tracker.ID))
                        {
                            trackersId.Add(tracker.ID);
                        }
                    }
                }

                if (_transmissionClient != null && IsConnected == true)
                {
                    await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                    {
                        IDs = ids,
                        TrackerRemove = trackersId.ToArray()
                    }, new CancellationToken());
                }
            }
        }

        public async void AddFromNewTrack()
        {
            if (await MessageBoxViewModel.ShowDialog(
                "Kebler will get trackers from newtrackon.com and add for each torrent. Are you sure?", manager,
                "Confirm action",
                Enums.MessageBoxDilogButtons.YesNo) == true)
            {
                if (_transmissionClient != null)
                {
                    var torents =
                        (await Get(_transmissionClient.TorrentGetAsync(new[] { TorrentFields.TRACKERS },
                                _cancelTokenSource.Token),
                            LocalizationProvider.GetLocalizedValue(nameof(Kebler.Resources.Strings.MW_StatusText_Torrents)))).Value;

                    var stringList =
                        await new WebClient().DownloadStringTaskAsync(new Uri("https://newtrackon.com/api/live"));

                    var trackersId = new List<string>();

                    using StringReader reader = new StringReader(stringList);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                            trackersId.Add(line);
                    }

                    foreach (var torrent in torents.Torrents)
                    {
                        var tracks = torrent.Trackers.Select(x => x.announce);
                        var toAdd = tracks.Except(trackersId);
                        var arr = toAdd.ToArray();
                        if (arr.Length > 0)
                        {
                            var resp = await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                            {
                                IDs = new[] { torrent.Id },
                                TrackerAdd = arr
                            }, new CancellationToken());

                            if (resp.Success == false)
                            {

                            }
                        }
                    }
                }
            }
        }

        public void ExportTorrents()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var torrent in _torrentList)
                {
                }
            }
        }

        public void MoreInfoShow()
        {
            ConfigService.Instanse.MoreInfoShow = IsShowMoreInfo;
            MoreInfoView.IsShowMoreInfoCheckNotify = !IsShowMoreInfo;
            ConfigService.Save();
            if (!IsShowMoreInfo)
                UpdateMoreInfoPosition(false);
        }

        #endregion
    }


    public partial class KeblerViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly object _syncTorrentList = new object();
        private readonly IWindowManager manager = new WindowManager();
        private readonly object syncObjKeys = new object();
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private BindableCollection<StatusCategory> _categoriesList = new BindableCollection<StatusCategory>();
        private Task? _checkerTask;
        private Server? _ConnectedServer;


        private Enums.Categories _filterCategory;
        private BindableCollection<FolderCategory> _folderCategory = new BindableCollection<FolderCategory>();
        private string? _isConnectedStatusText;
        private bool _isConnecting, _isDoingStuff, _isConnected, _isErrorOccuredWhileConnecting, _isSlowModeEnabled;
        private bool _isLongTaskRunning, isShowMoreInfo, _isAddWindOpened;

        private string? _uploadSpeed, _downloadSpeed, _filterText, _longStatusText;

        private BindableCollection<MenuItem> _languages = new BindableCollection<MenuItem>();
        private DateTimeOffset _longActionTimeStart;
        private CancellationTokenSource _moreInfoCancelTokeSource = new CancellationTokenSource();
        private double _MoreInfoColumnHeight, _oldMoreInfoColumnHeight, _minMoreInfoColumnHeight;

        private StatusCategory? _selectedCat;
        private int _selectedCategoryIndex = -1;
        private FolderCategory? _selectedFolder;
        private int _selectedFolderIndex;
        private Server? _SelectedServer;
        private TorrentInfo? _selectedTorrent;
        private List<Server>? _servers;
        private SessionInfo? _sessionInfo;
        private SessionSettings? _settings;
        private WindowState _state;
        private Statistic? _stats;
        private Bind<TorrentInfo> _torrentList = new Bind<TorrentInfo>();
        private TransmissionClient? _transmissionClient;
        private bool _isChangingLang;

        private static KeblerView? _view;

        //private Task? _whileCycleMoreInfoTask;
        private Task? _whileCycleTask;
        private TransmissionTorrents allTorrents = new TransmissionTorrents();
        private bool requested;
        private uint[] selectedIDs;
        private TorrentInfo[] SelectedTorrents = new TorrentInfo[0];
        private BindableCollection<MenuItem> servers = new BindableCollection<MenuItem>();

        public ICommand ShowConnectionManagerCommand => new DelegateCommand(ShowConnectionManager);
        public ICommand AddCommand => new DelegateCommand(Add);
        public ICommand AddMagnetCommand => new DelegateCommand(AddMagnet);
        public ICommand ExitCommand => new DelegateCommand(Exit);
        public ICommand RenameCommand => new DelegateCommand(Rename);
        public ICommand UnselectCommand => new DelegateCommand(Unselect);
        public ICommand FindCommand => new DelegateCommand(Find);
        public ICommand RemoveWithDataCommand => new DelegateCommand(RemoveWithData);
        public ICommand RemoveCommand => new DelegateCommand(Remove);
        public ICommand SpaceCommand => new DelegateCommand<object>(Space);

        private List<Server> ServersList
        {
            get { return _servers ??= GetServers(); }
        }


        public MoreInfoViewModel MoreInfoView { get; set; } = new MoreInfoViewModel(_view, null, null);

        public BindableCollection<FolderCategory> Categories
        {
            get => _folderCategory;
            set => Set(ref _folderCategory, value);
        }

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

        public TorrentInfo? SelectedTorrent
        {
            get => _selectedTorrent;
            set => Set(ref _selectedTorrent, value);
        }

        public BindableCollection<StatusCategory> CategoriesList
        {
            get => _categoriesList;
            set => Set(ref _categoriesList, value);
        }

        public Bind<TorrentInfo> TorrentList
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
            get => _isConnectedStatusText ?? string.Empty;
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

        public Server? ConnectedServer
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
            get => _longStatusText ?? string.Empty;
            set => Set(ref _longStatusText, value);
        }

        public bool IsSlowModeEnabled
        {
            get => _isSlowModeEnabled;
            set => Set(ref _isSlowModeEnabled, value);
        }

        public string DownloadSpeed
        {
            get => _downloadSpeed ?? string.Empty;
            set => Set(ref _downloadSpeed, value);
        }

        public string UploadSpeed
        {
            get => _uploadSpeed ?? string.Empty;
            set => Set(ref _uploadSpeed, value);
        }

        public string FilterText
        {
            get => _filterText ?? string.Empty;
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

        public static string HeaderTitle
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#elif PORTABLE
                return "Kebler [Portable]";
#else
                var assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo =
 System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta";
#endif
            }
        }

        public bool IsShowMoreInfo
        {
            get => isShowMoreInfo;
            set
            {
                Set(ref isShowMoreInfo, value);
                if (!value)
                    UpdateMoreInfoPosition(false);
            }
        }
    }
}