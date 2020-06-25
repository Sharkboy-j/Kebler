using Caliburn.Micro;
using Kebler.Models.Torrent;

namespace Kebler.ViewModels
{
    public class MoreInfoViewModel : PropertyChangedBase
    {
        private FilesTreeViewModel _filesTree = new FilesTreeViewModel();
        private TransmissionTorrentTrackerStats[] _trackerStats = new TransmissionTorrentTrackerStats[0];
        private TransmissionTorrentPeers[] _peers = new TransmissionTorrentPeers[0];
        private bool _loading, _isMore;
        private double _percentDone;
        private int _selectedCount;


        public void Update(TorrentInfo torrent)
        {
            FilesTree.UpdateFilesTree(torrent);
            PercentDone = torrent.PercentDone;

            TrackerStats = torrent.TrackerStats;
            Peers = torrent.Peers;
        }

        public uint[] id;

        public TransmissionTorrentTrackerStats[] TrackerStats
        {
            get => _trackerStats;
            set => Set(ref _trackerStats, value);
        }

        public TransmissionTorrentPeers[] Peers
        {
            get => _peers;
            set => Set(ref _peers, value);
        }

        public FilesTreeViewModel FilesTree
        {
            get => _filesTree;
            set => Set(ref _filesTree, value);
        }
        public bool Loading
        {
            get => _loading;
            set => Set(ref _loading, value);
        }
        public bool IsMore
        {
            get => _isMore;
            set => Set(ref _isMore, value);
        }
        public int SelectedCount
        {
            get => _selectedCount;
            set => Set(ref _selectedCount, value);
        }
        public double PercentDone
        {
            get => _percentDone;
            set => Set(ref _percentDone, value);
        }

        public void Clear()
        {
            FilesTree.Files = null;
        }
    }
}
