using Kebler.Models;
using Kebler.Services;
using Kebler.Services.Converters;
using Kebler.UI.Windows;
using Kebler.UI.Windows.Dialogs;
using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace Kebler.UI.ViewModels
{

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private List<Server> ServersList;
        private ConnectionManager CMWindow;
        private LiteCollection<Server> DBServers;
        private Statistic stats;
        private SessionInfo sessionInfo;
        private TransmissionClient transmissionClient;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected { get; set; } = false;
        public bool IsConnecting { get; set; } = false;
        public ObservableCollection<TorrentInfo> TorrentList { get; set; } = new ObservableCollection<TorrentInfo>();
        public TorrentInfo SelectedTorrent { get; set; }

        public bool IsErrorOccuredWhileConnecting { get; set; } = false;
        public int TorrentDataGridSelectedIndex { get; set; } = 0;


        #region StatusBarProps
        public string IsConnectedStatusText { get; set; } = string.Empty;
        public string DownloadSpeed { get; set; } = string.Empty;
        public string UploadSpeed { get; set; } = string.Empty;


        #endregion

        public MainWindowViewModel()
        {
            Start();
        }
        private void Start()
        {
            Task.Factory.StartNew(() =>
            {
                GetAllServer();
                InitConnection();
            });
        }

        private void InitConnection()
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                CMWindow = new ConnectionManager(DBServers);
                CMWindow.ShowDialog();
            });


        }
        private async void TryConnect(Server server)
        {
            Log.Info("Start Connection");
            IsErrorOccuredWhileConnecting = false;
            IsConnectedStatusText = "Connecting";
            try
            {
                IsConnecting = true;
                var pass = string.Empty;
                if (server.AskForPassword)
                {
                    pass = await GetPassword();
                    if (string.IsNullOrEmpty(pass)) return;

                }

                try
                {
                    transmissionClient = new TransmissionClient(server.FullUriPath, null, server.UserName, server.AskForPassword ? pass : SecureStorage.DecryptStringAndUnSecure(server.Password));

                    sessionInfo = await UpdateSessionInfo();
                    IsConnectedStatusText = $"Transmission {sessionInfo.Version} (RPC:{sessionInfo.RpcVersion})";

                    StartWhileCycle();

                    IsConnected = true;

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
        private async Task<string> GetPassword()
        {
            var pass = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
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

        private void StartWhileCycle()
        {
            new Task(async () =>
            {
                while (IsConnected)
                {
                    if (Application.Current.Dispatcher.HasShutdownStarted) return;

                    stats = await UpdateStats();
                    //sessionInfo = await UpdateSessionInfo();

                    var newTorrentsDataList = await UpdateTorrentList();

                    UpdateTorrentListAtUI(newTorrentsDataList);
                    UpdateStatsInfo();

                    Thread.Sleep(3000);
                }
            }).Start();
        }

        #region GetTorrentServerData
        private async Task<List<TorrentInfo>> UpdateTorrentList(string[] fields = null)
        {
            return (await transmissionClient.TorrentGetAsync(fields ?? TorrentFields.ALL_FIELDS)).Torrents.ToList();
        }
        private async Task<Statistic> UpdateStats()
        {
            return await transmissionClient.GetSessionStatisticAsync();
        }
        private async Task<SessionInfo> UpdateSessionInfo()
        {
            return await transmissionClient.GetSessionInformationAsync();

        }
        #endregion


        #region ParsersToUserFriendlyFormat
        private void UpdateTorrentListAtUI(List<TorrentInfo> list)
        {
            foreach (var item in list)
            {
                if (TorrentList.Any(x => x.ID == item.ID))
                {
                    var data = TorrentList.First(x => x.ID == item.ID);
                    var ind = TorrentList.IndexOf(data);
                    UpdateTorrentData(ind, item);
                }
                else
                {
                    Application.Current.Dispatcher.InvokeAsync(() => { TorrentList.Add(item); });

                }
            }
            //Dispatcher.InvokeAsync(() => { OnPropertyChanged(nameof(TorrentList)); });

            //Dispatcher.Invoke(delegate { TorrentsDataGrid.Items.Refresh(); });

        }
        private void UpdateTorrentData(int index, TorrentInfo newData)
        {

            if (SelectedTorrent.ID == newData.ID) SelectedTorrent = newData;

            TorrentList[index].RateUpload = newData.RateUpload;
            TorrentList[index].RateDownload = newData.RateDownload;
            TorrentList[index].AddedDate = newData.AddedDate;
            TorrentList[index].Pieces = newData.Pieces;
            TorrentList[index].PieceCount = newData.PieceCount;
            TorrentList[index].DownloadedEver = newData.DownloadedEver;
            TorrentList[index].PercentDone = newData.PercentDone;

        }

        private void UpdateStatsInfo()
        {
            var dSpeedText = BytesToUserFriendlySpeed.GetSizeString(stats.downloadSpeed);
            var uSpeedText = BytesToUserFriendlySpeed.GetSizeString(stats.uploadSpeed);

            var dSpeed = string.IsNullOrEmpty(dSpeedText) ? "0 b/s" : dSpeedText;
            var uSpeed = string.IsNullOrEmpty(uSpeedText) ? "0 b/s" : uSpeedText;

            DownloadSpeed = $"D: {dSpeed}";
            UploadSpeed = $"U: {uSpeed}";
        }
        #endregion




        #region Events

        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            new Task(() => { TryConnect(ServersList.FirstOrDefault()); }).Start();

        }
        #endregion


    }
}
