using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public uint[] tors;
        private TransmissionClient _transmissionClient;
        public TorrentPropsViewModel(TransmissionClient transmissionClient, uint[] ids)
        {
            _transmissionClient = transmissionClient;
            tors = ids;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var answ = await _transmissionClient.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS, new CancellationToken(), tors);
                    if (Application.Current.Dispatcher.HasShutdownStarted)
                        return;

                    if (answ != null)
                    {
                        var ti = answ.Torrents.FirstOrDefault();
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

        public async void Save()
        {
            try
            {
                await _transmissionClient.TorrentSetAsync(new TorrentSettings()
                {
                    DownloadLimited = MXD_Bool,
                    UploadLimited = MXU_Bool,
                    DownloadLimit = MaxDownSp,
                    UploadLimit = MaxUpSp,
                    SeedRatioLimit = SeedRation,
                    SeedIdleLimit = StopSeed,
                    IDs = tors,
                    PeerLimit = this.PeerLimit

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
                if (thisWindow != null)
                {
                    thisWindow.Owner = Application.Current.MainWindow;
                }
            }

            base.OnViewAttached(view, context);
        }

        private Visibility _isBusy;
        private string _name;
        private int _peerLimit, _stopSeed;
        private double _seedRation;
        private long _maxDownSp, _maxUpSp;
        private bool _MXD_Bool, _MXU_Bool, _SeedR_Bool, _StopSeed_Bool;

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

    }
}
