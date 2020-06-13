using System.Windows.Controls;

namespace Kebler.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for RemoveTorrentDialog.xaml
    /// </summary>
    public partial class RemoveTorrentDialog : UserControl
    {
        public bool WithData => (bool)RemoveWithDataCheckBox.IsChecked;
        public RemoveTorrentDialog(string[] names,bool witData = false)
        {
            InitializeComponent();
            Container.ItemsSource = names;
            RemoveWithDataCheckBox.IsChecked = witData;
        }
    }
}
