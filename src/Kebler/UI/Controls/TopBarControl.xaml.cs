using System.Windows;
using System.Windows.Controls;
using Kebler.Services;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for TopBarControl.xaml
    /// </summary>
    public partial class TopBarControl : UserControl
    {
        public TopBarControl()
        {
            InitializeComponent();
        }

        private void SetSettings()
        {
            var set = StorageRepository.GetSettingsList();
        }

        private void OpenConnectionManager(object sender, RoutedEventArgs e)
        {
            App.KeblerControl.OpenConnectionManager();
        }

        private void ConnectFirst(object sender, RoutedEventArgs e)
        {
            App.KeblerControl.Connect();
        }

        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            App.KeblerControl.AddTorrent();
        }
    }
}
