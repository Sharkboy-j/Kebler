using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Kebler.Models;
using Kebler.Services;
using log4net;
using LiteDB;
using Transmission.API.RPC;
using System.Threading.Tasks;
using Kebler.UI.Windows.Dialogs;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Transmission.API.RPC.Entity;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="MainWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ConnectionManager CMWindow;
        private LiteCollection<Server> DBServers;
        private List<Server> ServersList;
        private TransmissionClient transmissionClient;


        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected { get; set; } = false;
        public bool IsConnecting { get; set; } = false;

        public string StatusText { get; set; } = string.Empty;
        public bool IsErrorOccuredWhileConnecting { get; set; } = false;


        public MainWindow()
        {
            //log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            DataContext = this;
            Start();
        }

        private void Start()
        {
            Task.Factory.StartNew(() =>
            {
                GetAllServer();
                TryConnect();
            });
        }


        private void TryConnect()
        {
            if (ServersList.Count == 0)
            {
                ShowCm();
            }
            else
            {
                new Task(() => { TryConnect(ServersList.FirstOrDefault()); }).Start();

            }
        }



        private void ShowCm()
        {
            Dispatcher.Invoke(() =>
            {
                CMWindow = new ConnectionManager(DBServers);
                CMWindow.ShowDialog();
            });

          
        }
        private async void TryConnect(Server server)
        {
            Log.Info("Start Connection");
            IsErrorOccuredWhileConnecting = false;
            StatusText = "Connecting";
            try
            {
                IsConnecting = true;
                if (server.AskForPassword)
                {
                    var pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass)) return;
                    try
                    {
                        transmissionClient = new TransmissionClient(server.FullUriPath, null, server.UserName, server.AskForPassword? pass : SecureStorage.DecryptStringAndUnSecure(server.Password));

                        var sessionInfo = transmissionClient.GetSessionInformation();
                        StatusText = $"RPC: v.{sessionInfo.RpcVersion} Transmission: v.{sessionInfo.Version}";
                        await Dispatcher.InvokeAsync(async () =>
                        {
                            TorrentsDataGrid.ItemsSource = (await transmissionClient.TorrentGetAsync(TorrentFields.ALL_FIELDS)).Torrents;
                        });
                        
                        IsConnected = true;

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message, ex);
                        StatusText = ex.Message;
                        IsConnected = false;
                        IsErrorOccuredWhileConnecting = true;
                    }
                }
            }
            finally
            {
                IsConnecting = false;
            }



        }

        private async Task<string> GetPassword()
        {
            var pass = string.Empty;
            await Dispatcher.InvokeAsync(() =>
            {
                var dialog = new DialogPassword();
                pass = dialog.ShowDialog() == true ? dialog.ResponseText : string.Empty;
            });
            return pass;
        }


        private void GetAllServer()
        {
            DBServers = StorageRepository.ServersList();
            ServersList = new List<Server>(DBServers.FindAll() ?? new List<Server>());
            ServersList.LogServers();
        }


        #region Events

        #endregion

        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
         
            new Task(() => { TryConnect(ServersList.FirstOrDefault()); }).Start();
            
        }
    }
}
