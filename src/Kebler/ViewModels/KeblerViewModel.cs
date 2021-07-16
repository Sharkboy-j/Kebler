using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;
using Kebler.Resources;
using Kebler.Services;
using Kebler.TransmissionCore;
using Kebler.Views;
using Microsoft.AppCenter.Crashes;
using static Kebler.Models.Messages;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using ILog = log4net.ILog;
using ListView = System.Windows.Controls.ListView;
using LogManager = log4net.LogManager;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TorrentFields = Kebler.Models.Torrent.TorrentFields;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : BaseScreen,
        IHandle<LocalizationCultureChangesMessage>,
        IHandle<ReconnectRequested>,
        IHandle<ReconnectAllowed>,
        IHandle<ConnectedServerChanged>,
        IHandle<ServersUpdated>,
        IHandle<ShowMoreInfoChanged>


    {
        private readonly IEventAggregator _eventAggregator;

        public KeblerViewModel()
        {
            _eventAggregator = new EventAggregator();
        }


        public KeblerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            _eventAggregator.SubscribeOnPublishedThread(this);

            var menus = InitMenu(LocalizationManager.CultureList, LanguageChanged, Thread.CurrentThread.CurrentCulture);

            Languages = new BindableCollection<System.Windows.Controls.MenuItem>(menus);
            Servers = new BindableCollection<MenuItem>(ReInitServers(StorageRepository.GetServersList().FindAll() ?? new List<Server>(), ServerClicked));

            SelectedFolderIndex = -1;

            SelectedCat = CategoriesList.First();


            App.Instance.KeblerVM = this;
        }

        public Task HandleAsync(ConnectedServerChanged message, CancellationToken cancellationToken)
        {
            if (message.Server == null)
            {
                foreach (var item in Servers) item.IsChecked = false;
                return Task.CompletedTask;
            }

            foreach (var item in Servers)
                if (item.Tag is Server srv)
                    if (srv.Equals(message.Server))
                    {
                        item.IsChecked = true;
                        break;
                    }

            return Task.CompletedTask;
        }

        public Task HandleAsync(LocalizationCultureChangesMessage message, CancellationToken cancellationToken)
        {
            _isChangingLang = true;

            try
            {

                foreach (var item in Languages) item.IsChecked = Equals((CultureInfo)item.Tag, message.Culture);

                if (IsConnected && _stats != null)
                    IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Strings.Stats_Uploaded))} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Strings.Stats_Downloaded))}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                            $"      {LocalizationProvider.GetLocalizedValue(nameof(Strings.Stats_ActiveTime))}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";

                var ind = SelectedCategoryIndex;


                SelectedCategoryIndex = ind;
            }
            finally
            {
                _isChangingLang = false;
            }



            return Task.CompletedTask;
        }

        public Task HandleAsync(ReconnectAllowed message, CancellationToken cancellationToken)
        {
            requested = false;
            GetServerAndInitConnection();
            return Task.CompletedTask;
        }

        public async Task HandleAsync(ReconnectRequested message, CancellationToken cancellationToken)
        {
            if (IsConnected)
            {
                requested = true;
                _cancelTokenSource.Cancel();
                IsConnecting = true;
                if (_SelectedServer != message.Server)
                    _SelectedServer = message.Server;
            }
            else if (IsConnecting)
            {
                _cancelTokenSource.Cancel();
                await Task.Delay(1000);
                _SelectedServer = message.Server;
                GetServerAndInitConnection();
            }
            else
            {
                IsConnecting = true;
                _SelectedServer = message.Server;
                GetServerAndInitConnection();
            }
        }

        public Task HandleAsync(ServersUpdated message, CancellationToken cancellationToken)
        {
            Servers = new BindableCollection<MenuItem>(ReInitServers(StorageRepository.GetServersList().FindAll() ?? new List<Server>(), ServerClicked));

            return Task.CompletedTask;
        }

        public Task HandleAsync(ShowMoreInfoChanged message, CancellationToken cancellationToken)
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
            Task.Factory.StartNew(GetServerAndInitConnection, cancellationToken);

            return base.OnActivateAsync(cancellationToken);
        }


        public void ShowConnectionManager()
        {
            var sw = new Stopwatch();
            sw.Start();
            manager.ShowDialogAsync(new ConnectionManagerViewModel(_eventAggregator));
            sw.Stop();
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

                MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Strings.ConfigApllyError)), manager);
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
                else if (_view.MoreInfoColumn.ActualHeight != 0)
                {
                    _oldMoreInfoColumnHeight = _view.MoreInfoColumn.ActualHeight;
                    _view.MoreInfoColumn.MinHeight = 0;
                    _view.MoreInfoColumn.Height = new GridLength(0);
                    return;
                }

                else if (_view.MoreInfoColumn.ActualHeight == 0D && !string.IsNullOrEmpty(FilterText))
                {
                    ClearFilter();
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



        private async Task<TransmissionResponse<T>> Get<T>(Task<TransmissionResponse<T>> task, string statusText)
        {
            try
            {
                _isLongTaskRunning = true;
                _longActionTimeStart = DateTimeOffset.Now;
                LongStatusText = statusText;
                var resp = await task;
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

        /// <summary>
        /// Clear filter textBox and unselect curent folder
        /// </summary>
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


        public async void SetLoc()
        {
            if (IsConnected && SelectedTorrent != null)
            {
                var question = SelectedTorrents.Length > 1
                    ? LocalizationProvider.GetLocalizedValue(nameof(Strings.SetLocForMany)).Replace("%d", selectedIDs.Length.ToString())
                    : LocalizationProvider.GetLocalizedValue(nameof(Strings.SetLocOnce));

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

                            if (IsResponseStatusOk(resp))
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
                var dialog = new DialogBoxViewModel(Strings.MSG_LinkOrMagnet, string.Empty, false, Enums.MessageBoxDilogButtons.OkCancel, LocalizationProvider.GetLocalizedValue(nameof(Strings.Error_EmptyString)));

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


        public void CopyMagnet()
        {
            if (SelectedTorrent != null)
                Clipboard.SetText(SelectedTorrent.MagnetLink);
            else
                MessageBoxViewModel.ShowDialog("Please select torrent");
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
                "Kebler will get trackers from newtrackon.com and add all to each torrent. Are you sure?", manager,
                "Confirm action",
                Enums.MessageBoxDilogButtons.YesNo) == true)
            {
                if (_transmissionClient != null)
                {
                    var torents =
                        (await Get(_transmissionClient.TorrentGetAsync(new[] { TorrentFields.TRACKERS },
                                _cancelTokenSource.Token),
                            LocalizationProvider.GetLocalizedValue(nameof(Strings.MW_StatusText_Torrents)))).Value;

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
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

        private BindableCollection<System.Windows.Controls.MenuItem> _languages = new BindableCollection<System.Windows.Controls.MenuItem>();
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
        private Statistic _stats;
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
        private BindableCollection<System.Windows.Controls.MenuItem> servers = new BindableCollection<System.Windows.Controls.MenuItem>();

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


        public MoreInfoViewModel? MoreInfoView { get; set; }

        public BindableCollection<FolderCategory> Categories
        {
            get => _folderCategory;
            set => Set(ref _folderCategory, value);
        }

        public BindableCollection<System.Windows.Controls.MenuItem> Languages
        {
            get => _languages;
            set => Set(ref _languages, value);
        }

        public BindableCollection<System.Windows.Controls.MenuItem> Servers
        {
            get => servers;
            set => Set(ref servers, value);
        }

        public TorrentInfo? SelectedTorrent
        {
            get => _selectedTorrent;
            set => Set(ref _selectedTorrent, value);
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

        public Server? SelectedServer
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
                _eventAggregator.PublishOnUIThreadAsync(new ConnectedServerChanged(value));
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