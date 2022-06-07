using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Caliburn.Micro;
using Humanizer.Localisation;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Domain.Models;
using Kebler.Domain.Models.Events;
using Kebler.Localisation;
using Kebler.Services;
using Kebler.Transmission.Models;
using Kebler.Transmission.Models.Arguments;
using Kebler.UI;
using Kebler.UI.Models;
using Kebler.ViewModels.DialogWindows;
using Kebler.Views;
using Microsoft.Win32;

namespace Kebler.ViewModels
{
    public partial class KeblerViewModel : BaseScreen
    {
        private readonly IWindowManager _manager;
        private readonly ILogger _logger;
        private readonly ICustomLocalizationProvider _localizationProvider;
        private readonly ITorrentClientsWorker _worker;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _config;
        private readonly ILocalizationManager _localizationManager;
        private static KeblerView _view;
        private ITorrent[] SelectedTorrents = Array.Empty<ITorrent>();
        private CancellationTokenSource _moreInfoCancelTokeSource = new();
        private uint[] selectedIDs;
        private bool _isChangingLang;
        private static object _lockObject = new object();

        public ICommand SpaceCommand => new DelegateCommand<object>(Space);
        public ICommand AddCommand => new DelegateCommand(Add);
        public ICommand AddMagnetCommand => new DelegateCommand(AddMagnet);
        public ICommand ExitCommand => new DelegateCommand(Exit);
        public ICommand RenameCommand => new DelegateCommand(Rename);
        public ICommand UnselectCommand => new DelegateCommand(Unselect);
        public ICommand FindCommand => new DelegateCommand(Find);
        public ICommand RemoveWithDataCommand => new DelegateCommand(RemoveWithData);
        public ICommand RemoveCommand => new DelegateCommand(Remove);

        /// <summary>
        /// Kebler window Title with version.
        /// </summary>
        public new string Title
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#elif PORTABLE
                return "Kebler [Portable]";
#elif RELEASE
 var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fileVersionInfo =
                    System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion} Beta x64";
#endif
            }
        }

        private WindowState _state;

        private ITorrent _selectedTorrent;
        public ITorrent SelectedTorrent
        {
            get => _selectedTorrent;
            set => Set(ref _selectedTorrent, value);
        }

        public WindowState State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        private BindableCollection<MenuItem> _serversMenu = new();

        public BindableCollection<MenuItem> Servers
        {
            get => _serversMenu;
            set => Set(ref _serversMenu, value);
        }

        /// <summary>
        /// Preference to show/hide categories count.
        /// </summary>
        private bool _showCategoriesCount = true;

        public bool ShowCategoriesCount
        {
            get => _showCategoriesCount;
            set => Set(ref _showCategoriesCount, value);
        }

        private bool _isShowMoreInfo;

        public bool IsShowMoreInfo
        {
            get => _isShowMoreInfo;
            set
            {
                Set(ref _isShowMoreInfo, value);
                //if (!value)
                //    UpdateMoreInfoPosition(false);
            }
        }

        private int _selectedCategoryIndex = -1;

        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set => Set(ref _selectedCategoryIndex, value);
        }

        private StatusCategory _selectedCat;

        public StatusCategory SelectedCat
        {
            get => _selectedCat;
            set => Set(ref _selectedCat, value);
        }

        public KeblerViewModel(
            IWindowManager manager,
            ILogger logger,
            ICustomLocalizationProvider localizationProvider,
            ITorrentClientsWorker worker,
            IEventAggregator eventAggregator,
            IConfigService config,
            ILocalizationManager localizationManager)
        {
            _manager = manager;
            _logger = logger;
            _localizationProvider = localizationProvider;
            _worker = worker;
            _eventAggregator = eventAggregator;
            _config = config;
            _localizationManager = localizationManager;

            CategoriesList = new BindableCollection<StatusCategory>()
            {
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_AllTorrents)),
                    Cat = Kebler.Domain.Models.Categories.All
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_Downloading)),
                    Cat = Domain.Models.Categories.Downloading
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_Active)),
                    Cat = Domain.Models.Categories.Active
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_InActive)),
                    Cat = Domain.Models.Categories.Inactive
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_Ended)),
                    Cat = Domain.Models.Categories.Ended
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_Stopped)),
                    Cat = Domain.Models.Categories.Stopped
                },
                new()
                {
                    Title = _localizationProvider.GetLocalizedValue(nameof(Strings.Cat_Error)),
                    Cat = Domain.Models.Categories.Error
                }
            };
            Servers = new BindableCollection<MenuItem>(ReInitServers(StorageRepository.GetServersList().FindAll() ?? new List<Server>(), ServerClicked));

            Application.Current.Dispatcher.Invoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(CategoriesList, _lockObject);
                BindingOperations.EnableCollectionSynchronization(Servers, _lockObject);
            });

            SelectedCat = CategoriesList.First();
        }

        private void ApplyConfig()
        {
            try
            {
                if (_view != null)
                {
                    _view.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    _view.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                    //_view.CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                    _view.Width = ConfigService.DefaultSettingsInstanse.MainWindowWidth;
                    _view.Height = ConfigService.DefaultSettingsInstanse.MainWindowHeight;
                    //_view.WindowState = ConfigService.DefaultSettingsInstanse.MainWindowState;
                    IsShowMoreInfo = ConfigService.DefaultSettingsInstanse.MoreInfoShow;
                    ShowCategoriesCount = ConfigService.DefaultSettingsInstanse.ShowCategoryCount;
                }
                else
                {
                    throw new Exception($" Filed '{nameof(_view)}' is null");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                //Crashes.TrackError(ex);

                _manager.ShowMessageBoxDialog(
                    _localizationProvider.GetLocalizedValue(nameof(Strings.ConfigApllyError)));
            }
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Task.Factory.StartNew(GetServerAndInitConnection, cancellationToken);

            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            _view = view as KeblerView;
            if (_view != null)
            {
                //_view.MoreView.FileTreeViewControl.OnFileStatusUpdate += FileTreeViewControl_OnFileStatusUpdate;
                //MoreInfoView = new MoreInfoViewModel(_view, UpdateMoreInfoPosition, _eventAggregator);
                //_view.MoreView.FileTreeViewControl.Checked += MoreInfoView.MakeRecheck;
            }

            base.OnViewAttached(view, context);
            ApplyConfig();
        }

        #region CaliburnEvents

        public async Task ShowConnectionManager()
        {
            var vm = IoC.Get<ConnectionManagerViewModel>();
            await _manager.ShowDialogAsync(vm);
        }

        public async void TorrentChanged(ListView obj, TorrentInfo inf)
        {
            try
            {
                SelectedTorrents = obj.SelectedItems.Cast<ITorrent>().ToArray();

                _moreInfoCancelTokeSource?.Cancel();
                _moreInfoCancelTokeSource = new CancellationTokenSource();

                await Task.Delay(250, _moreInfoCancelTokeSource.Token);

                if (_moreInfoCancelTokeSource.Token.IsCancellationRequested)
                    return;
                selectedIDs = SelectedTorrents.Select(x => x.Id).ToArray();

                if (ConfigService.DefaultSettingsInstanse.MoreInfoShow)
                {
                    //UpdateMoreInfoPosition(SelectedTorrents.Any());
                    //UpdateMoreInfoView(_moreInfoCancelTokeSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
            }
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

            ProcessDataTask();
        }

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

        public void Remove() => RemoveTorrent();
        public void RemoveWithData() => RemoveTorrent(true);

        public void Space(object obj)
        {
            if (IsConnected && obj is ITorrent tr)
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

        public void Unselect()
        {
            //UpdateMoreInfoPosition(false);
        }

        /// <summary>
        /// Rename torrent.
        /// </summary>
        public async void Rename()
        {
            //if (selectedIDs.Length > 1)
            //{
            //    await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_OnlyOneTorrent)), manager);
            //    return;
            //}

            //if (selectedIDs.Length < 1)
            //{
            //    await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_SelectOneTorrent)), manager);
            //    return;
            //}

            //if (!IsConnected || SelectedTorrent is null) return;

            //var selectedTorrent = SelectedTorrent;
            //var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.MSG_InterNewName)), selectedTorrent.Name, false);

            //var result = await manager.ShowDialogAsync(dialog);

            //var newName = dialog.Value.ToString();

            //if (result is not true || _transmissionClient is null || newName is null)
            //    return;

            //var resp = await _transmissionClient.TorrentRenamePathAsync(selectedTorrent.Id, selectedTorrent.Name, newName,
            //    _cancelTokenSource.Token);
            //resp.ParseTransmissionReponse(Log);
        }

        public void Exit()
        {
            _logger.Ui();
            TryCloseAsync();
        }

        public async void AddMagnet()
        {
            //if (IsConnected && _transmissionClient != null)
            //{
            //    var dialog = new DialogBoxViewModel(Strings.MSG_LinkOrMagnet, string.Empty, false, Enums.MessageBoxDilogButtons.OkCancel, LocalizationProvider.GetLocalizedValue(nameof(Strings.Error_EmptyString)));

            //    var result = await manager.ShowDialogAsync(dialog);

            //    if (result == true && dialog.Value is string str && !string.IsNullOrEmpty(str))
            //    {
            //        var newTr = new NewTorrent
            //        {
            //            Filename = str,
            //            Paused = false
            //        };

            //        await _transmissionClient.TorrentAddAsync(newTr, _cancelTokenSource.Token);
            //    }
            //}
        }

        public void Find()
        {
            Execute.OnUIThread(() => { _view.FilterTextBox.Focus(); });
        }

        #region Mi actions

        public void Report()
        {
            _logger.Ui(nameof(Report));
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/Rebell81/Kebler/issues")
            {
                CreateNoWindow = true,
                UseShellExecute = true
            });
        }

        public void OpenLogs()
        {
            _logger.Ui(nameof(OpenLogs));

            try
            {
                _logger.Info($"Try start => cmd {Logger.LogFileInfo.DirectoryName}");
                var p = new Process();
                if (@Logger.LogFileInfo.DirectoryName != null)
                    p.StartInfo = new ProcessStartInfo(@Logger.LogFileInfo.DirectoryName)
                    {
                        UseShellExecute = true
                    };
                p.Start();
            }
            catch (Exception ex)
            {
#if RELEASE

//                System.IO
//Path.GetFullPath(String path)
//System.IO.FileInfo
//System.IO.FileInfo..ctor(String originalPath, String fullPath, String fileName, Boolean isNormalized)
//System.IO.FileInfo
//System.IO.FileInfo..ctor(String fileName)
//Kebler.Views
//TopBarView.OpenLogs(Object sender, RoutedEventArgs e)
//System.Windows
//RoutedEventHandlerInfo.InvokeHandler(Object target, RoutedEventArgs routedEventArgs)
//System.Windows
//EventRoute.InvokeHandlersImpl(Object source, RoutedEventArgs args, Boolean reRaised)
//System.Windows
//UIElement.RaiseEventImpl(DependencyObject sender, RoutedEventArgs args)
//System.Windows
//UIElement.RaiseEvent(RoutedEventArgs e)
//System.Windows.Controls
//MenuItem.InvokeClickAfterRender(Object arg)
//System.Windows.Threading
//ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
//System.Windows.Threading
//ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)

                //App.Log.Error(ex);
#endif
            }


        }

        public async void About()
        {
            _logger.Ui(nameof(About));

            var about = IoC.Get<AboutViewModel>();

            await _manager.ShowDialogAsync(about);
        }

        public void Check()
        {
#if RELEASE
            //App.Log.Ui(nameof(Check));

           // System.Threading.Tasks.Task.Run(Updater.CheckUpdates);
#endif
        }

        public void Contact()
        {
            _logger.Ui(nameof(Contact));

            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/Rebell81")
            {
                CreateNoWindow = true,
                UseShellExecute = true
            });
        }

        #endregion

        #endregion

        private Task OpenTorrent(IEnumerable<string> names)
        {
            return Task.CompletedTask;
            //foreach (var item in names)
            //{
            //    if (string.IsNullOrEmpty(item))
            //        continue;

            //    if (item.StartsWith("magnet"))
            //    {
            //        var tr = new NewTorrent
            //        {
            //            Filename = item
            //        };
            //        await _transmissionClient?.TorrentAddAsync(tr, _cancelTokenSource.Token)!;
            //    }
            //    else
            //    {

            //        var downs = _torrentList.Select(c => new { c.Name, c.Id }).Select(c => (c.Name, c.Id));
            //        var dialog = new AddTorrentViewModel(item, _transmissionClient, _settings,
            //            _torrentList, _folderCategory, _eventAggregator,
            //            downs, RemoveTorrent,
            //            ref _isAddWindOpened, ref _view);


            //        await manager.ShowDialogAsync(dialog);


            //        _isAddWindOpened = false;

            //        if (dialog.Result == true)
            //        {
            //            if (dialog.TorrentResult.Value.Status == Enums.AddTorrentStatus.Added)
            //                TorrentList.Add(new TorrentInfo(dialog.TorrentResult.Value.ID)
            //                {
            //                    Name = dialog.TorrentResult.Value.Name,
            //                    HashString = dialog.TorrentResult.Value.HashString
            //                });
            //        }
            //        else
            //        {
            //            return;
            //        }
            //    }
            //}
        }

        private void RemoveTorrent(bool removeData = false)
        {
            //if (selectedIDs.Length > 0)
            //{
            //    var toRemove = selectedIDs;
            //    var dialog =
            //        new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), toRemove,
            //                ref _transmissionClient, removeData)
            //            { Owner = Application.Current.MainWindow };

            //    if (dialog.ShowDialog() == true && dialog.Result == Enums.RemoveResult.Ok)
            //        lock (_syncTorrentList)
            //        {
            //            foreach (var rm in toRemove)
            //            {
            //                var itm = TorrentList.First(x => x.Id == rm);
            //                TorrentList.Remove(itm);

            //                allTorrents.Torrents = allTorrents.Torrents.Where(val => val.Id == rm).ToArray();
            //            }

            //            if (allTorrents.Clone() is TransmissionTorrents data)
            //                ProcessParsingTransmissionResponse(data);
            //        }
            //}
        }

        public async void Pause(uint[] ids)
        {
            //if (IsConnected && _transmissionClient != null)
            //{
            //    var id = ids ?? selectedIDs;
            //    var resp = await _transmissionClient.TorrentStopAsync(id, _cancelTokenSource.Token);
            //    if (resp.Success)
            //    {
            //        foreach (var item in id)
            //        {
            //            TorrentList.First(x => x.Id == item).Status = 0;
            //        }
            //    }

            //    resp.ParseTransmissionReponse(Log);
        }
        public async void Start(uint[] ids)
        {
            //if (IsConnected && _transmissionClient != null)
            //{
            //    var id = ids ?? selectedIDs;
            //    //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            //    var resp = await _transmissionClient.TorrentStartAsync(id, _cancelTokenSource.Token);
            //    if (resp.Success)
            //    {
            //        foreach (var item in id)
            //        {
            //            var tor = TorrentList.FirstOrDefault(x => x.Id == item);
            //            if (tor != null)
            //            {
            //                tor.Status = tor.PercentDone == 1D ? 6 : 4;
            //            }
            //        }
            //    }

            //    resp.ParseTransmissionReponse(Log);
            //}
        }

        public IEnumerable<MenuItem> ReInitServers(
            IEnumerable<Server> dbServers,
            RoutedEventHandler serverClicked)
        {
            if (dbServers is null)
            {
                throw new ArgumentNullException(nameof(dbServers));
            }

            if (serverClicked is null)
            {
                throw new ArgumentNullException(nameof(serverClicked));
            }

            var servers = new List<MenuItem>();

            foreach (var server in dbServers)
            {
                var menuItem = new MenuItem { Header = server.Title, Tag = server };
                menuItem.Click += serverClicked;
                menuItem.IsCheckable = true;

                servers.Add(menuItem);
            }

            return servers;
        }

        /// <summary>
        /// MenuButton server clicked event-action.
        /// </summary>
        private void ServerClicked(object sender, RoutedEventArgs e)
        {
            _logger.Ui();

            if (sender is not MenuItem {Tag: IServer server})
                return;

            foreach (var item in Servers) item.IsChecked = false;

            _eventAggregator.PublishOnUIThreadAsync(new ReconnectRequested(server));
        }
    }


}
