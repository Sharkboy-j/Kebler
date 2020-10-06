using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Resources;
using Kebler.TransmissionCore;

namespace Kebler.ViewModels
{
    public class TorrentPropsViewModel : Screen
    {
        private readonly TransmissionClient _transmissionClient;
        private Visibility _isBusy;
        private long _maxDownSp, _maxUpSp;
        private bool _MXD_Bool, _MXU_Bool, _SeedR_Bool, _StopSeed_Bool;
        private string _name;
        private int _peerLimit, _stopSeed;
        private double _seedRation;
        private uint[] tors;
        private BindableCollection<TransmissionTorrentTrackers> _trackers;

        private int _trackerIndex;
        private TransmissionTorrentTrackers? _selectedTracker;


        public TorrentPropsViewModel()
        {
        }

        public TorrentPropsViewModel(TransmissionClient transmissionClient, uint[] ids)
        {
            _transmissionClient = transmissionClient;
            tors = ids;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var answ = await _transmissionClient.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS,
                        new CancellationToken(), tors);
                    if (Application.Current.Dispatcher.HasShutdownStarted)
                        return;

                    if (answ != null && answ.Torrents.Length > 0)
                    {
                        var ti = answ.Torrents.First();
                        Name = ids.Length > 1 ? $"{ids.Length} {Strings.TP_TorrentsSelected}" : ti?.Name;
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

        public TransmissionTorrentTrackers? SelectedTracker
        {
            get => _selectedTracker;
            set => Set(ref _selectedTracker, value);
        }


        public void Cancel()
        {
            TryCloseAsync();
        }


        public async void Save()
        {
            try
            {
                await _transmissionClient.TorrentSetAsync(new TorrentSettings
                {
                    DownloadLimited = MXD_Bool,
                    UploadLimited = MXU_Bool,
                    DownloadLimit = MaxDownSp,
                    UploadLimit = MaxUpSp,
                    SeedRatioLimit = SeedRation,
                    SeedIdleLimit = StopSeed,
                    IDs = tors,
                    PeerLimit = PeerLimit,
                    TrackerAdd = toAdd.ToArray(),
                    TrackerRemove = toRm.ToArray()
                }, new CancellationToken());
                await TryCloseAsync();
            }
            catch (Exception ex)
            {
                //ignore
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
        private List<int> toRm = new List<int>();

        public async void AddTracker()
        {
            var mgr = IoC.Get<IWindowManager>();

            var dialog = new DialogBoxViewModel(Strings.AddTrackerTitile, string.Empty, false);
            var result = await mgr.ShowDialogAsync(dialog);

            var maxId = Trackers.Max(x => x.ID) + 1;
            var tracker = new TransmissionTorrentTrackers()
            {
                announce = dialog.Value,
                ID = maxId
            };
            if (result == true && !Trackers.Contains(tracker))
            {
                Trackers.Add(tracker);
                toAdd.Add(dialog.Value);
            }
        }
    }
}