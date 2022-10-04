using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Kebler.Core.Domain.Intrfaces;
using Kebler.Core.Models;
using Kebler.Core.Models.Arguments;
using Kebler.Services;
using Kebler.TransmissionTorrentClient;
using Kebler.TransmissionTorrentClient.Models;

namespace Kebler.ViewModels
{
    public class TorrentPropsViewModel : Screen
    {
        private readonly IRemoteTorrentClient _transmissionClient = new TransmissionClient(string.Empty);
        private Visibility _isBusy;
        private long _maxDownSp, _maxUpSp;
        private bool _MXD_Bool, _MXU_Bool, _SeedR_Bool, _StopSeed_Bool;
        private string _name = string.Empty;
        private int _peerLimit, _stopSeed;
        private double _seedRation;
        private uint[] tors = new List<uint>().ToArray();
        private BindableCollection<TransmissionTorrentTrackers> _trackers = new BindableCollection<TransmissionTorrentTrackers>();

        private int _trackerIndex;
        private TransmissionTorrentTrackers _selectedTracker;
        private readonly IWindowManager _manager = new WindowManager();


        public TorrentPropsViewModel()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transmissionClient">TransmissionClient.</param>
        /// <param name="ids">Torrent ids</param>
        /// <param name="manager">IWindowManager</param>
        public TorrentPropsViewModel(IRemoteTorrentClient transmissionClient, uint[] ids, IWindowManager manager)
        {
            _transmissionClient = transmissionClient;
            tors = ids;
            _manager = manager;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var answ = await _transmissionClient.TorrentGetAsyncWithId(TorrentFields.ALL_FIELDS,
                        new CancellationToken(), tors);
                    if (Application.Current.Dispatcher.HasShutdownStarted)
                        return;

                    if (answ != null && answ.Torrents.Length > 0)
                    {
                        var ti = answ.Torrents.First();
                        Name = ids.Length > 1 ? $"{ids.Length} {LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TP_TorrentsSelected))}" : ti?.Name;
                        MaxDownSp = ti.DownloadLimit;
                        MaxUpSp = ti.UploadLimit;
                        PeerLimit = ti.PeerLimit;
                        StopSeed = ti.SeedIdleLimit;
                        SeedRation = ti.SeedRatioLimit;

                        MXD_Bool = ti.DownloadLimited;
                        MXU_Bool = ti.UploadLimited;
                        //SeedR_Bool = ti.SeedRatioMode;
                        //StopSeed_Bool = ti.SeedIdleMode;

                        Trackers = new BindableCollection<TransmissionTorrentTrackers>(ti.Trackers);
                    }
                }
                finally
                {
                    IsBusy = Visibility.Collapsed;
                }
            });
        }

        public void Cancel()
        {
            TryCloseAsync();
        }

        public void Copy()
        {
            Clipboard.SetText(SelectedTracker.announce);
        }



        public async void Save()
        {
            try
            {
                var sett = new TorrentSettings
                {
                    DownloadLimited = MXD_Bool,
                    UploadLimited = MXU_Bool,
                    DownloadLimit = MaxDownSp,
                    UploadLimit = MaxUpSp,
                    SeedRatioLimit = SeedRation,
                    SeedIdleLimit = StopSeed,
                    IDs = tors,
                    PeerLimit = PeerLimit,
                    TrackerAdd = toAdd?.Count <= 0 ? null : toAdd?.ToArray(),
                    TrackerRemove = toRm?.Count <= 0 ? null : toRm?.ToArray(),
                };

                var resp = await _transmissionClient.TorrentSetAsync(sett, new CancellationToken());

                if (resp.Success)
                {
                    await TryCloseAsync();
                }
                else
                {
                    await MessageBoxViewModel.ShowDialog(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.TorretPropertiesSetError)), _manager);
                }


            }
            finally
            {
                IsBusy = Visibility.Collapsed;
            }
        }


        protected override void OnViewAttached(object view, object context)
        {
            if (view is DependencyObject dependencyObject)
            {
                var thisWindow = Window.GetWindow(dependencyObject);
                if (thisWindow != null) thisWindow.Owner = Application.Current.MainWindow;
            }

            base.OnViewAttached(view, context);
        }


        public void DeleteTracker()
        {
            if (SelectedTracker != null)
            {
                var tr = SelectedTracker;
                Trackers.Remove(tr);
                toRm.Add(tr.ID);
                toAdd.Remove(tr.announce);
            }
        }

        private List<string> toAdd = new List<string>();
        private List<uint> toRm = new List<uint>();

        public async void AddTracker()
        {
            var mgr = IoC.Get<IWindowManager>();

            var dialog = new DialogBoxViewModel(LocalizationProvider.GetLocalizedValue(nameof(Resources.Strings.AddTrackerTitile)), string.Empty, false);
            var result = await mgr.ShowDialogAsync(dialog);

            var maxId = Trackers.Max(x => x.ID) + 1;


            if (dialog.Value is not null)
            {
                var tracker = new TransmissionTorrentTrackers()
                {
                    announce = dialog.Value.ToString(),
                    ID = maxId
                };
                if (result == true && !Trackers.Contains(tracker))
                {
                    Trackers.Add(tracker);
                    toAdd.Add(dialog.Value.ToString());
                }
            }
        }


        #region props
        public BindableCollection<TransmissionTorrentTrackers> Trackers
        {
            get => _trackers;
            set => Set(ref _trackers, value);
        }

        public Visibility IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public long MaxDownSp
        {
            get => _maxDownSp;
            set => Set(ref _maxDownSp, value);
        }

        public long MaxUpSp
        {
            get => _maxUpSp;
            set => Set(ref _maxUpSp, value);
        }

        public int PeerLimit
        {
            get => _peerLimit;
            set => Set(ref _peerLimit, value);
        }

        public int StopSeed
        {
            get => _stopSeed;
            set => Set(ref _stopSeed, value);
        }

        public double SeedRation
        {
            get => _seedRation;
            set => Set(ref _seedRation, value);
        }


        public bool MXD_Bool
        {
            get => _MXD_Bool;
            set => Set(ref _MXD_Bool, value);
        }

        public bool MXU_Bool
        {
            get => _MXU_Bool;
            set => Set(ref _MXU_Bool, value);
        }

        public bool SeedR_Bool
        {
            get => _SeedR_Bool;
            set => Set(ref _SeedR_Bool, value);
        }

        public bool StopSeed_Bool
        {
            get => _StopSeed_Bool;
            set => Set(ref _StopSeed_Bool, value);
        }

        public int TrackerIndex
        {
            get => _trackerIndex;
            set => Set(ref _trackerIndex, value);
        }

        public TransmissionTorrentTrackers SelectedTracker
        {
            get => _selectedTracker;
            set => Set(ref _selectedTracker, value);
        }
        #endregion
    }
}