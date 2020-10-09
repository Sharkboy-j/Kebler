using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kebler.Models;
using Kebler.UI.CSControls.TreeListView;

namespace Kebler.UI.Controls
{
    /// <summary>
    ///     Interaction logic for FilesTreeView.xaml
    /// </summary>
    public partial class FilesTreeView
    {

        public FilesTreeView()
        {
            InitializeComponent();

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk && chk.Tag is TorrentFile trnt)
            {
                Set(trnt, chk.IsChecked);
                RecalParent(trnt);
                tree.Items.Refresh();
            }
        }

        void Set(TorrentFile trn, bool? val)
        {
            if (trn.Children.Count > 0)
            {
                foreach (var item in trn.Children)
                {
                    Set(item, val);
                }
            }
            trn.Checked = val;
        }

        void RecalParent(TorrentFile trn)
        {
            if (trn.Parent != null)
            {
                //allFalse
                if (trn.Parent.Children.All(x => x.Checked == false))
                {
                    trn.Parent.Checked = false;
                }
                //allTrue
                else if (trn.Parent.Children.All(x => x.Checked == true))
                {
                    trn.Parent.Checked = true;
                }
                else
                {
                    trn.Parent.Checked = null;
                }
                RecalParent(trn.Parent);
            }
        }

        private void tree_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (tree.SelectedItem is TreeNode node && node.Tag is TorrentFile trnt)
                {
                    trnt.Checked = trnt.Checked == null ? false : !trnt.Checked;
                    Set(trnt, trnt.Checked);
                    RecalParent(trnt);

                    tree.Items.Refresh();

                }
            }

        }

       
    }
}