using Caliburn.Micro;

namespace Kebler.ViewModels
{
    public class MoreInfoViewModel : PropertyChangedBase
    {
        private FilesTreeViewModel _filesTree = new FilesTreeViewModel();
        private bool _loading, _isMore;
        private double _percentDone;
        private int _selectedCount;


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
