using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
