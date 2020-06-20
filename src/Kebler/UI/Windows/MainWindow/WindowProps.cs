using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Entity;
using Kebler.Services;
using Kebler.TransmissionCore;
using Kebler.UI.CSControls.MultiTreeView;
using LiteDB;
using log4net;
using Enums = Kebler.Models.Enums;
// ReSharper disable UnusedMember.Global

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        private TransmissionTorrents allTorrents = new TransmissionTorrents();
        private object _syncTorrentList = new object();
        private List<Server> _servers;
        private HotKey[] RegisteredKeys;
        private object syncObjKeys = new object();
        private Server _connectedServer;
        private LiteCollection<Server> _dbServers;
        private Statistic _stats;
        private SessionInfo _sessionInfo;
        private ConnectionManager _cmWindow;
        private Task _whileCycleTask;
        private DateTimeOffset _longActionTimeStart;
        private bool _isLongTaskRunning;
        private Task _checkerTask;
        private Enums.Categories _filterCategory;
        private string[] WorkingParams;
        private uint[] selectedIDs;

        public TransmissionClient _transmissionClient;
        public SessionSettings _settings;
        public event PropertyChangedEventHandler PropertyChanged;





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





        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public Server SelectedServer { get; set; }
        public bool IsSlowModeEnabled { get; set; }
        public bool IsDoingStuff { get; set; }
        public string LongStatusText { get; set; }
        public string FilterText { get; set; }
        public TorrentInfo[] SelectedTorrents { get; set; }
        public bool IsErrorOccuredWhileConnecting { get; set; }




        public bool IsDoingStuffReverse => !IsDoingStuff;





        public ObservableCollection<TorrentInfo> TorrentList { get; set; } = new ObservableCollection<TorrentInfo>();
        public ObservableCollection<FolderCategory> Categories { get; set; } = new ObservableCollection<FolderCategory>();
        public MoreInfoModel MoreInfo { get; set; } = new MoreInfoModel();
        public string IsConnectedStatusText { get; set; } = string.Empty;
        public string DownloadSpeed { get; set; } = string.Empty;
        public string UploadSpeed { get; set; } = string.Empty;
        public int TorrentDataGridSelectedIndex { get; set; } = 1;



        public string HeaderTitle
        {
            get
            {
#if DEBUG
                return "Kebler [DEBUG]";
#else
                var assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                return $"{nameof(Kebler)} {fileVersionInfo.FileVersion}";
#endif
            }
        }
    }
}
