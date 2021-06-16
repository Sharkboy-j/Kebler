using Kebler.Models.Interfaces;

namespace Kebler.UI.Dialogs
{
    public partial class AddTorrentView : IAddTorrent
    {
        public AddTorrentView()
        {
            InitializeComponent();
        }

        public object FilesTreeView => FileTreeViewControl.tree;
    }
}