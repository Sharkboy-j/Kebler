using Kebler.Models;
using Kebler.Services;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Transmission.API.RPC;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for ConnectionManager.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class ConnectionManager : INotifyPropertyChanged
    {

        #region Public props
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelectedServerTabEnabled { get; set; } = true;
        public Server SelectedServer { get; set; }// = new Server();
        public ObservableCollection<Server> ServerList = new ObservableCollection<Server>();

        public bool IsTesting { get; set; }
        public string ConnectStatusResult { get; set; } = string.Empty;
        public SolidColorBrush ConnectStatusColor { get; set; }
        #endregion

        private static readonly ILog Log = LogManager.GetLogger(typeof(App));
        private LiteCollection<Server> DbServersList;
        private TransmissionClient _client;

        public ConnectionManager(ref LiteCollection<Server> dbServersList)
        {
            Log.Info("Open ConnectionManager");

            InitializeComponent();
            DbServersList = dbServersList;
            DataContext = this;

            GetServers();
        }




        //TODO: Add background loading
        private void GetServers(int selectedId = -1)
        {
            var items = DbServersList?.FindAll() ?? new List<Server>();

            ServerList = new ObservableCollection<Server>(items);
            //LogServers();

            ServersListBox.ItemsSource = ServerList;

            if (selectedId != -1)
            {
                SelectedServer = ServerList.FirstOrDefault(x => x.Id == selectedId);

                foreach (var item in ServersListBox.Items)
                {
                    if (!(item is Server server)) continue;
                    if (server.Id != selectedId) continue;

                    var ind = ServersListBox.Items.IndexOf(item);
                    ServersListBox.SelectedIndex = ind;
                    break;
                }
            }
            else
            {
                ServersListBox.SelectedIndex = 0;
            }

        }



        private void AddNewServer_ButtonClick(object sender, RoutedEventArgs e)
        {
            var id = ServerList.Count == 0 ? 1 : ServerList[ServerList.Count - 1].Id + 1;

            var server = new Server() { Id = id, Title = $"Transmission Server {ServerList.Count + 1}", AskForPassword = false, AuthEnabled = false };


            DbServersList.Insert(server);
            Log.Info($"Add new Server {server}");
            //var result = DbServersList.(x => x.Title);
            GetServers(server.Id);
        }

        private void ServersListBox_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServersListBox.SelectedItems.Count != 1) return;

            SelectedServer = ServersListBox.SelectedItems[0] as Server;
            ServerPasswordBox.Password =
                SecureStorage.DecryptStringAndUnSecure(SelectedServer?.Password);
            ConnectStatusResult = string.Empty;

        }

        //TODO: Add to collection and storage
        private void SaveServer_ButtonClick(object sender, RoutedEventArgs e)
        {

            Log.Info($"Try save Server {SelectedServer}");

            if (ValidateServer(SelectedServer, out var error))
            {
                SelectedServer.Password = SelectedServer.AskForPassword ? string.Empty : SecureStorage.EncryptString(ServerPasswordBox.Password);

                var res = DbServersList.Upsert(SelectedServer);
                Log.Info($"UpsertResult: {res}, Error: {error}");
                GetServers(SelectedServer.Id);
            }
            else
            {
                Log.Error($"SaveError: {error}");
                System.Windows.MessageBox.Show(error.ToString());
            }

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
            //TODO: Check ipadress port
        }
        private void CloseServer_ButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedServer = null;
            ServersListBox.SelectedIndex = -1;
        }


        //TODO: add background removing
        private void RemoveServer_ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;

            Log.Info($"Try remove server: {SelectedServer}");



            var result = StorageRepository.GetServersList().Delete(SelectedServer.Id);
            Log.Info($"RemoveResult: {result}");
            if (!result)
            {
                //TODO: Add string 
                System.Windows.MessageBox.Show("RemoveErrorContent");
            }

            SelectedServer = null;
            GetServers();
        }

        private async void TestConnection_ButtonClick(object sender, RoutedEventArgs e)
        {

            string pass = null;
            if (SelectedServer.AskForPassword)
            {
                var dialog = new MessageBox(true,"Enter password",true, string.Empty);
                dialog.Owner = this;
                try
                {
                    if (dialog.ShowDialog() == true)
                    {
                        pass = dialog.Value;
                    }
                    else
                    {
                        return;
                    }
                }
                finally
                {

                    dialog.Value = null;
                    dialog = null;
                }
            }


            IsTesting = true;
            var result = await TesConnection(pass);
            ConnectStatusResult = result
                ? Kebler.Resources.Windows.CM_TestConnectionGood
                : Kebler.Resources.Windows.CM_TestConnectionBad;
            
            ConnectStatusColor = (result
                ? new SolidColorBrush() { Color = Colors.Green }
                : new SolidColorBrush() { Color = Colors.Red });

            IsTesting = false;
        }

        private async Task<bool> TesConnection(string pass)
        {
            Log.Info("Start TestConnection");
            var scheme = SelectedServer.SSLEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

            var uri = new UriBuilder(scheme, SelectedServer.Host, SelectedServer.Port, SelectedServer.RPCPath);

            try
            {
                var user = (bool)AuthEnabled.IsChecked ? SelectedServer.UserName : null;
                var pswd = (bool)AuthEnabled.IsChecked ? pass ?? ServerPasswordBox.Password : null;

                var _client = new TransmissionClient(uri.Uri.AbsoluteUri, null, user, pswd);

                var sessionInfo = await _client.GetSessionInformationAsync();
                if (sessionInfo == null)
                    throw new Exception("Error while testing");

                await _client.CloseSessionAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                return false;
            }
            finally
            {
                _client = null;
            }


            return true;
        }

        private void SSL_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedServer != null)
                SelectedServer.SSLEnabled = true;
        }

        private void SSL_Uncheked(object sender, RoutedEventArgs e)
        {
            if (SelectedServer != null)
                SelectedServer.SSLEnabled = false;
        }

        private void CustomWindow_Closing(object sender, CancelEventArgs e)
        {
            if (_client != null)
            {
                _client = null;
            }

            if (ServerList.Count != 0)
            {
                App.KeblerControl.Connect();
            }

        }
    }
}
