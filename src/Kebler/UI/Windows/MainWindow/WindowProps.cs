using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Models;
using Kebler.Services;
using LiteDB;
using log4net;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Transmission.API.RPC.Entity;
using Transmission.API.RPC.Response;
using Enums = Kebler.Models.Enums;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow
    {
        private List<Server> _servers;
        private HotKey[] RegisteredKeys;
        private object syncObjKeys = new object();

        private Server _connectedServer;
        public Server ConnectedServer
        {
            get => _connectedServer;
            set
            {
                _connectedServer = value;
                App.ChangeConnectedServer(_connectedServer);
            }
        }

        public List<Server> ServersList
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

        public void UpdateServers()
        {
            _dbServers = StorageRepository.GetServersList();
            _servers = _dbServers.FindAll().ToList();
        }

        private LiteCollection<Server> _dbServers;
        private Statistic _stats;
        private SessionInfo _sessionInfo;
        public TransmissionClient _transmissionClient;
        private ConnectionManager _cmWindow;
        private Task _whileCycleTask;
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private DateTimeOffset _longActionTimeStart;
        private bool _isLongTaskRunning;
        private Task _checkerTask;
        private Enums.Categories _filterCategory;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public Server SelectedServer { get; set; }
        public bool IsSlowModeEnabled { get; set; }

        public bool IsDoingStuff { get; set; }
        // ReSharper disable once UnusedMember.Global
        public bool IsDoingStuffReverse => !IsDoingStuff;

        public string LongStatusText { get; set; }
        public string FilterText { get; set; }

        public string HeaderTitle
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#else
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion}";
#endif
            }
        }

        public ObservableCollection<TorrentInfo> TorrentList { get; set; } = new ObservableCollection<TorrentInfo>();
        private object _syncTorrentList = new object();


        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private TransmissionTorrents allTorrents = new TransmissionTorrents();
        private string[] WorkingParams;


        public SessionSettings _settings;
        public TorrentInfo[] SelectedTorrents { get; set; }

        public bool IsErrorOccuredWhileConnecting { get; set; }


#region StatusBarProps
        public string IsConnectedStatusText { get; set; } = string.Empty;
        public string DownloadSpeed { get; set; } = string.Empty;
        public string UploadSpeed { get; set; } = string.Empty;
#endregion

    }
}
