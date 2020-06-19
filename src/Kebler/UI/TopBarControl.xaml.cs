using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Services;
using MessageBox = Kebler.Dialogs.MessageBox;

namespace Kebler.UI
{
    /// <summary>
    /// Interaction logic for TopBarControl.xaml
    /// </summary>
    public partial class TopBarControl
    {

        public TopBarControl()
        {
            InitializeComponent();

            App.ConnectionChanged += App_ConnectionChanged;
            App.ServerListChanged += UpdateServerList;
            foreach (var item in LocalizationManager.CultureList)
            {
                var dd = new MenuItem { Header = item.EnglishName, Tag = item };
                dd.Click += langChanged;
                dd.IsCheckable = true;
                dd.IsChecked = Equals(Thread.CurrentThread.CurrentCulture, item);
                LangMenu.Items.Add(dd);
            }

            UpdateServerList();

        }

        public void UpdateServerList()
        {
            App.Instance.KeblerControl.UpdateServers();

            ServersMenu.Items.Clear();
            foreach (var item in App.Instance.KeblerControl.ServersList)
            {
                var dd = new MenuItem { Header = item.Title, Tag = item };
                dd.Click += ServerMenuItemCkicked;
                dd.IsCheckable = true;
                dd.IsChecked = item.Equals(App.Instance.KeblerControl.SelectedServer);
                ServersMenu.Items.Add(dd);
            }
        }


        private void App_ConnectionChanged(Server srv)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (srv == null)
                {
                    foreach (MenuItem mi in ServersMenu.Items)
                    {
                        mi.IsChecked = false;
                    }
                }
                else
                {
                    foreach (MenuItem mi in ServersMenu.Items)
                    {
                        mi.IsChecked = ((Server)mi.Tag).Equals(srv);
                    }
                }
            });
        }


        private void ServerMenuItemCkicked(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem ss)) return;

            App.Instance.KeblerControl.SelectedServer = ss.Tag as Server;
            App.Instance.KeblerControl.ReconnectToNewServer();
        }


        private void langChanged(object sender, RoutedEventArgs e)
        {
            foreach (var item in LangMenu.Items)
            {
                if (item is MenuItem itm)
                {
                    itm.IsChecked = false;
                }
            }

            if (sender is MenuItem mi)
            {
                LocalizationManager.CurrentCulture = (CultureInfo)mi.Tag;
                mi.IsChecked = true;
            }
        }

        private void OpenConnectionManager(object sender, RoutedEventArgs e)
        {
            App.Instance.KeblerControl.ShowConnectionManager();
        }

        private void ConnectFirst(object sender, RoutedEventArgs e)
        {
            App.Instance.KeblerControl.InitConnection();
        }

        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            App.Instance.KeblerControl.AddTorrent();
        }

        private void Report(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/JeremiSharkboy/Kebler/issues") { CreateNoWindow = true });
        }

        private void About(object sender, RoutedEventArgs e)
        {

            var dialog = new About(Application.Current.MainWindow);
            dialog.ShowDialog();


        }

        private void Check(object sender, RoutedEventArgs e)
        {
            App.Instance.Check(true);
        }

        private void Contact(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://t.me/jeremiSharkboy") { CreateNoWindow = true });
        }
    }
}
