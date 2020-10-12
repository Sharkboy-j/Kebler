using Kebler.Models.Interfaces;
using System.Windows;

namespace Kebler.UI.Dialogs
{
    public partial class AddTorrentView : IAddTorrent
    {
        public AddTorrentView()
        {
            InitializeComponent();
        }

        public object FilesTreeView => this.FileTreeViewControl.tree;
    }
}