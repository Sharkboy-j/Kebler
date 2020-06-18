using Kebler.Models.Tree;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for TorrentMoreInfo.xaml
    /// </summary>
    public partial class TorrentMoreInfo
    {
        public TorrentMoreInfo()
        {
            InitializeComponent();
        }

        public void Update(MultiselectionTreeViewItem items)
        {
            var ddd = new MultiselectionTreeViewItem() { IsExpanded = true };
            ddd.Children.Add(items);
            TreeView.RootItem = ddd;
        }
    }
}
