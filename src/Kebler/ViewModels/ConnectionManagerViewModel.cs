using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Kebler.Models;
using Kebler.Models.Interfaces;
using Kebler.Models.Torrent.Common;
using Kebler.Services;
using Kebler.TransmissionCore;
using LiteDB;
using Microsoft.AppCenter.Crashes;

namespace Kebler.ViewModels
{
    public partial class ConnectionManagerViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;

        public ConnectionManagerViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
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


        private void GetServers(int selectedId = -1)
        {
            _dbServersList = StorageRepository.GetServersList();

            var items = _dbServersList?.FindAll() ?? new List<Server>();

            ServerList.Clear();
            ServerList.AddRange(items);

            ServerIndex = selectedId != -1 ? selectedId : 0;
        }


        public void Add()
        {
            Log.Ui();

            var server = new Server
            { Title = $"Transmission Server {ServerList.Count + 1}", AskForPassword = false, AuthEnabled = false };
            _dbServersList.Insert(server);

            Log.Info($"Add new Server {server}");

            ServerList.Add(server);

            _eventAggregator.PublishOnUIThreadAsync(new Messages.ServersUpdated());

            // App.InvokeServerListChanged();
        }

        public void Remove()
        {
            Log.Ui();

            if (SelectedServer == null) return;


            Log.Info($"Try remove server: {SelectedServer}");

            var result = StorageRepository.GetServersList().Delete(SelectedServer.Id);
            Log.Info($"RemoveResult: {result}");
            if (!result)
                //TODO: Add string 
                MessageBoxViewModel.ShowDialog("RemoveErrorContent", manager);

            var ind = ServerIndex -= 1;

            //if (App.Instance.KeblerControl.SelectedServer.Id == SelectedServer.Id)
            //{
            //    App.Instance.KeblerControl.Disconnect();
            //}
            _eventAggregator.PublishOnUIThreadAsync(new Messages.ServersUpdated());

            SelectedServer = null;
            GetServers();
            ServerIndex = ind;
        }

        public void Save()
        {
            Log.Ui();

            Log.Info($"Try save Server {SelectedServer}");

            if (ValidateServer(SelectedServer, out var error))
            {
                SelectedServer.Password = SelectedServer.AskForPassword
                    ? string.Empty
                    : SecureStorage.EncryptString(_view.pwd.Password);

                var res = _dbServersList.Upsert(SelectedServer);
                Log.Info($"UpsertResult: {res}, Error: {error}");
                GetServers(SelectedServer.Id);
            }
            else
            {
                Log.Error($"SaveError: {error}");
                MessageBox.Show(error.ToString());
            }

            _eventAggregator.PublishOnUIThreadAsync(new Messages.ServersUpdated());
        }

        public void Cancel()
        {
            Log.Ui();
            SelectedServer = null;
            ServerIndex = -1;
        }

        private static bool ValidateServer(Server server, out Enums.ValidateError error)
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

        private async Task<bool> CheckResponse(ITransmissionReponse resp)
        {
            if (resp.WebException != null)
            {
                //TODO: to IWindowManager manager = new WindowManager();
                var msg = resp.WebException.Status switch
                {
                    WebExceptionStatus.NameResolutionFailure =>
                        $"{LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.EX_Host))} '{SelectedServer.FullUriPath}'",
                    _ => $"{resp.WebException.Status} {Environment.NewLine} {resp.WebException?.Message}"
                };


                await MessageBoxViewModel.ShowDialog(msg, manager, LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.Error)));

                return false;
            }

            if (resp.CustomException != null)
                return false;
            return true;
        }

        public async void Test()
        {
            Log.Ui();
            var st = new Stopwatch();
            st.Start();

            IsTesting = true;

            string pswd = null;
            if (SelectedServer.AskForPassword)
            {
                var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.DialogBox_EnterPWD)), string.Empty, true);
                var res = await manager.ShowDialogAsync(dialog);
                if (res == false)
                {
                    IsTesting = false;
                    return;
                }

                pswd = dialog.Value.ToString();
            }

            var result = await TesConnection(pswd);
            st.Stop();
            Log.Trace(st.Elapsed);

            if (result.Item2 != null)
            {

                await manager.ShowDialogAsync(new MessageBoxViewModel(result.Item2.Message, string.Empty,
                    Enums.MessageBoxDilogButtons.Ok, true));

                ConnectStatusResult =
                    LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.CM_TestConnectionBad));

                ConnectStatusColor = new SolidColorBrush {Color = Colors.Red};
            }
            else
            {
                var answer = await CheckResponse(result.Item1);
                    
                ConnectStatusResult = answer
                    ? LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.CM_TestConnectionGood))
                    : LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.CM_TestConnectionBad));

                ConnectStatusColor = answer
                    ? new SolidColorBrush { Color = Colors.Green }
                    : new SolidColorBrush { Color = Colors.Red };
            }
        }

        private async Task<(TransmissionResponse, Exception)> TesConnection(string pass)
        {
            Log.Info("Start TestConnection");
            var scheme = SelectedServer.SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

            var uri = new UriBuilder(scheme, SelectedServer.Host, SelectedServer.Port, SelectedServer.RpcPath);

            try
            {
                var user = SelectedServer.AuthEnabled ? SelectedServer.UserName : null;
                var pswd = SelectedServer.AuthEnabled ? pass ?? _view.pwd.Password : null;

                _client = new TransmissionClient(uri.Uri.AbsoluteUri, null, user, pswd);

                var sessionInfo = await _client.GetSessionInformationAsync(CancellationToken.None);

                Log.Info("Test connection OK");
                return (sessionInfo.Response, null);
            }
            catch (Exception ex)
            {
                Log.Info("Test connection failed");
                Log.Error(ex);
                Crashes.TrackError(ex);

                return (null, ex);
            }
            finally
            {
                Log.Info("Test connection end");
                _client = null;
                IsTesting = false;
            }
        }

        public void ServerChanged()
        {
            if (SelectedServer != null)
            {
                if (SelectedServer.AuthEnabled && !SelectedServer.AskForPassword)
                    _view.pwd.Password = SecureStorage.DecryptStringAndUnSecure(SelectedServer.Password);
                else
                    _view.pwd.Password = string.Empty;
            }
        }
    }

    public partial class ConnectionManagerViewModel
    {
        private static readonly Kebler.Services.Interfaces.ILog Log = Kebler.Services.Log.Instance;
        private readonly IWindowManager manager = new WindowManager();
        private TransmissionClient _client;
        private SolidColorBrush _connectStatusColor;
        private string _connectStatusResult;
        private LiteCollection<Server> _dbServersList;
        private bool _isTesting;
        private Server _selectedServer;
        private int _serverIndex;
        private BindableCollection<Server> _serverList = new BindableCollection<Server>();
        private IConnectionManager _view;


        public BindableCollection<Server> ServerList
        {
            get => _serverList;
            set => Set(ref _serverList, value);
        }

        public int ServerIndex
        {
            get => _serverIndex;
            set => Set(ref _serverIndex, value);
        }

        public Server SelectedServer
        {
            get => _selectedServer;
            set => Set(ref _selectedServer, value);
        }

        public bool IsTesting
        {
            get => _isTesting;
            set => Set(ref _isTesting, value);
        }


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
    }
}