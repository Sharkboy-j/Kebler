using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kebler.Models.Interfaces;
using Kebler.Models.Tree;

namespace Kebler.UI.Controls
{
    /// <summary>
    ///     Interaction logic for FilesTreeView.xaml
    /// </summary>
    public partial class FilesTreeView
    {
        public delegate void FileStatusUpdateHandler(uint[] wanted, uint[] unwanted, bool status);

        public FilesTreeView()
        {
            InitializeComponent();
        }

        public event FileStatusUpdateHandler OnFileStatusUpdate;


        private void FolderCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk)
            {
                if (Trree.SelectedItems.Count > 1)
                    foreach (MultiselectionTreeViewItem item in Trree.SelectedItems)
                        item.IsChecked = (bool) chk.IsChecked;
                else
                    Trree.UnselectAll();


                var file = chk.Tag as MultiselectionTreeViewItem;
                Trree.SelectAndFocus(file);


                if (DataContext is IFilesTreeView dd)
                    OnFileStatusUpdate?.Invoke(dd.getFilesWantedStatus(true), dd.getFilesWantedStatus(false),
                        (bool) file.IsChecked);
            }
        }


        private void Trree_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var val = ((MultiselectionTreeViewItem) Trree.SelectedValue).IsChecked;

                if (val == null)
                    ((MultiselectionTreeViewItem) Trree.SelectedValue).IsChecked = false;
                else
                    ((MultiselectionTreeViewItem) Trree.SelectedValue).IsChecked = !val;
            }
        }
    }
}