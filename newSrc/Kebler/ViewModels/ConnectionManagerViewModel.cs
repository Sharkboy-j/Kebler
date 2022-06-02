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
using Humanizer.Localisation;
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
        public ConnectionManagerViewModel(IEventAggregator eventAggregator, ILogger logger, ICustomLocalizationProvider localizationProvider, ITorrentClientsWorker worker)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
            _localizationProvider = localizationProvider;
            _worker = worker;
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

        public void Remove()
        {
            _logger.Ui();

            if (SelectedServer == null)
                return;

            _logger.Info($"Try remove server: {SelectedServer}");

            var result = StorageRepository.GetServersList().Delete(SelectedServer.Id);
            _logger.Info($"RemoveResult: {result}");
            //if (!result)
                //TODO: Add string 
                //TODO: Uncomment when dialogs be ready
                //MessageBoxViewModel.ShowDialog("RemoveErrorContent", manager);

            var ind = ServerIndex -= 1;
            
            _eventAggregator.PublishOnUIThreadAsync(new ServersUpdated());

            SelectedServer = null;
            GetServers();
            ServerIndex = ind;
        }

        public void Save()
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
                MessageBox.Show(error.ToString());
            }

            _eventAggregator.PublishOnUIThreadAsync(new ServersUpdated());
        }

        public async void Test()
        {
            _logger.Ui();
            var st = new Stopwatch();
            st.Start();

            IsTesting = true;

            string pswd = null;
            //TODO: Uncomment when dialog be ready
            //if (SelectedServer.AskForPassword)
            //{

            //var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.DialogBox_EnterPWD)), string.Empty, true);
            //var res = await manager.ShowDialogAsync(dialog);
            //if (res == false)
            //{
            //IsTesting = false;
            //return;
            //}

            // pswd = dialog.Value.ToString();
            //}

            var (result, resultException) = await TesConnection(pswd);
            st.Stop();
            _logger.Trace(st);

            if (resultException != null)
            {

                //await manager.ShowDialogAsync(new MessageBoxViewModel(result.Item2.Message, string.Empty,
                //    Enums.MessageBoxDilogButtons.Ok, true));

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
            //TODO: CheckUpdates ipadress port
        }

        private async Task<(bool, Exception)> TesConnection(string pass)
        {
            try
            {
                //var pswd = SelectedServer.AuthEnabled ? pass ?? _view.PasswordBox.Password : null;
                //var user = SelectedServer.AuthEnabled ? SelectedServer.UserName : null;

                var result = await _worker.CheckConnectionAsync(SelectedServer);

                return result;
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