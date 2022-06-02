using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Models;
using Kebler.Domain.Models.Events;
using Kebler.Interfaces;
using Kebler.Services;
using Kebler.Transmission.Models;
using LiteDB;

namespace Kebler.ViewModels
{
    public class ConnectionManagerViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly ICustomLocalizationProvider _localizationProvider;
        private readonly ITorrentClientsWorker _worker;
        private readonly IWindowManager _manager;


        private BindableCollection<IServer> _serverList = new();
        private LiteCollection<Server> _dbServersList;
        private IServer _selectedServer;
        private IConnectionManager _view;
        private int _serverIndex;
        private bool _isTesting;
        private string _connectStatusResult;
        private SolidColorBrush _connectStatusColor;

        #region Caliburn properties

        public SolidColorBrush ConnectStatusColor
        {
            get => _connectStatusColor;
            set => Set(ref _connectStatusColor, value);
        }

        public string ConnectStatusResult
        {
            get => _connectStatusResult;
            set => Set(ref _connectStatusResult, value);
        }

        public IServer SelectedServer
        {
            get => _selectedServer;
            set => Set(ref _selectedServer, value);
        }

        public BindableCollection<IServer> ServerList
        {
            get => _serverList;
            set => Set(ref _serverList, value);
        }

        public int ServerIndex
        {
            get => _serverIndex;
            set => Set(ref _serverIndex, value);
        }

        public bool IsTesting
        {
            get => _isTesting;
            set => Set(ref _isTesting, value);
        }

        #endregion

        [ImportingConstructor]
        public ConnectionManagerViewModel(
            IEventAggregator eventAggregator,
            ILogger logger,
            ICustomLocalizationProvider localizationProvider,
            ITorrentClientsWorker worker,
            IWindowManager manager)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
            _localizationProvider = localizationProvider;
            _worker = worker;
            _manager = manager;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            GetServers();
            return base.OnActivateAsync(cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            _view = view as IConnectionManager;
            if (view is DependencyObject dependencyObject)
            {
                var thisWindow = Window.GetWindow(dependencyObject);
                if (thisWindow != null) thisWindow.Owner = Application.Current.MainWindow;
            }

            base.OnViewAttached(view, context);
        }

        #region Caliburn Events

        public void Add()
        {
            _logger.Ui();

            var server = new Server
            { Title = $"Transmission Server {ServerList.Count + 1}", AskForPassword = false, AuthEnabled = false };
            _dbServersList.Insert(server);

            _logger.Info($"Add new Server {server}");

            ServerList.Add(server);

            _eventAggregator.PublishOnUIThreadAsync(new ServersUpdated());

            // App.InvokeServerListChanged();
        }

        public async void Remove()
        {
            _logger.Ui();

            if (SelectedServer == null)
                return;

            _logger.Info($"Try remove server: {SelectedServer}");

            var result = StorageRepository.GetServersList().Delete(SelectedServer.Id);
            _logger.Info($"RemoveResult: {result}");

            //TODO: Add string 
            if (!result)
                _ = await _manager.ShowMessageBoxDialog("RemoveErrorContent");


            var ind = ServerIndex -= 1;

            await _eventAggregator.PublishOnUIThreadAsync(new ServersUpdated());

            SelectedServer = null;
            GetServers();
            ServerIndex = ind;
        }

        public async void Save()
        {
            _logger.Ui();
            _logger.Info($"Try save Server {SelectedServer}");

            if (ValidateServer(SelectedServer, out var error))
            {
                SelectedServer.Password = SelectedServer.AskForPassword ?
                    string.Empty
                    : SecureStorage.EncryptString(_view.PasswordBox.Password);

                var res = _dbServersList.Upsert((Server)SelectedServer);
                _logger.Info($"UpsertResult: {res}, Error: {error}");
                GetServers(SelectedServer.Id);
            }
            else
            {
                _logger.Error($"SaveError: {error}");
                _ = await _manager.ShowMessageBoxDialog(error.ToString());
            }

            await _eventAggregator.PublishOnUIThreadAsync(new ServersUpdated());
        }

        public async void Test()
        {
            _logger.Ui();
            var st = new Stopwatch();
            st.Start();

            IsTesting = true;

            string password = null;
            if (SelectedServer.AskForPassword)
            {
                var (resultPassword, dialogResult) = await _manager.AskPasswordDialog("DialogBox_EnterPWD"/*nameof(Resources.Strings.DialogBox_EnterPWD)*/);
                if (dialogResult == true)
                {
                    password = resultPassword;
                }
                else
                {
                    IsTesting = false;
                    return;
                }
            }

            var (result, resultException) = await TesConnection(password);
            st.Stop();
            _logger.Trace(st);

            if (resultException != null)
            {

                if (resultException is WebException webExc)
                {
                    var msg = webExc.Status switch
                    {
                        WebExceptionStatus.NameResolutionFailure =>
                            $"{_localizationProvider.GetLocalizedValue("EX_Host"/*nameof(Resources.Strings.EX_Host)*/)} '{SelectedServer.FullUriPath}'",
                        _ => $"{webExc.Status} {Environment.NewLine} {webExc?.Message}"
                    };

                    _ = await _manager.ShowMessageBoxDialog(msg);
                }
                else
                {
                    _ = await _manager.ShowMessageBoxDialog(resultException.Message);
                }

                ConnectStatusResult =
                    _localizationProvider.GetLocalizedValue("CM_TestConnectionBad"/*nameof(Resources.Strings.CM_TestConnectionBad)*/);

                ConnectStatusColor = new SolidColorBrush { Color = Colors.Red };
            }
            else
            {
                ConnectStatusResult = result ?
                    _localizationProvider.GetLocalizedValue("CM_TestConnectionGood"/*nameof(Resources.Strings.CM_TestConnectionGood)*/)
                    : _localizationProvider.GetLocalizedValue("CM_TestConnectionBad"/*nameof(Resources.Strings.CM_TestConnectionBad)*/);

                ConnectStatusColor = result ?
                    new SolidColorBrush { Color = Colors.Green }
                    : new SolidColorBrush { Color = Colors.Red };
            }
        }

        public void Cancel()
        {
            _logger.Ui();
            SelectedServer = null;
            ServerIndex = -1;
        }


        public void ServerChanged()
        {
            if (SelectedServer != null)
            {
                if (SelectedServer.AuthEnabled && !SelectedServer.AskForPassword)
                    _view.PasswordBox.Password = SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password);
                else
                    _view.PasswordBox.Password = string.Empty;
            }
        }

        #endregion

        private void GetServers(int selectedId = -1)
        {
            _dbServersList = StorageRepository.GetServersList();

            var serversFromDb = _dbServersList?.FindAll() ?? new List<Server>();

            ServerList.Clear();
            ServerList.AddRange(serversFromDb);

            ServerIndex = selectedId != -1 ? selectedId : 0;
        }

        private static bool ValidateServer(IServer server, out Enums.ValidateError error)
        {
            error = Enums.ValidateError.Ok;
            if (string.IsNullOrEmpty(server.Title))
            {
                error = Enums.ValidateError.TitileEmpty;
                return false;
            }

            if (!string.IsNullOrEmpty(server.Host)) return true;

            error = Enums.ValidateError.IpOrHostError;
            return false;
        }

        private async Task<(bool, Exception)> TesConnection(string pass)
        {
            try
            {
                var result = await _worker.CheckConnectionAsync(SelectedServer);

                return result.Item2 != null ? (false, result.Item2) : result;
            }
            catch (Exception ex)
            {
                //Log.Info("Test connection failed");
                _logger.Error(ex);
                //Crashes.TrackError(ex);

                return (false, ex);
            }
            finally
            {
                IsTesting = false;
            }
        }


    }

}