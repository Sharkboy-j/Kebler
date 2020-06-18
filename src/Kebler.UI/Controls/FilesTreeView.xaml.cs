using System.Windows;
using System.Windows.Controls;
using Kebler.Models.Tree;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for FilesTreeView.xaml
    /// </summary>
    public partial class FilesTreeView
    {
        public FilesTreeView()
        {
            InitializeComponent();
        }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk)
            {
                if (Trree.SelectedItems.Count > 1)
                {
                    foreach (MultiselectionTreeViewItem item in Trree.SelectedItems)
                    {
                        item.IsChecked = (bool)chk.IsChecked;
                    }
                }
                else
                    Trree.UnselectAll();

                var ss = chk.Tag as MultiselectionTreeViewItem;
                Trree.SelectAndFocus(ss);
            }

        }

    }
}
