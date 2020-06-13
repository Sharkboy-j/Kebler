using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models;
using Kebler.Services;
using MessageBox = Kebler.UI.Windows.MessageBox;

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
            App.KeblerControl.UpdateServers();

            ServersMenu.Items.Clear();
            foreach (var item in App.KeblerControl.ServersList)
            {
                var dd = new MenuItem { Header = item.Title, Tag = item };
                dd.Click += ServerMenuItemCkicked;
                dd.IsCheckable = true;
                dd.IsChecked = item.Equals(App.KeblerControl.SelectedServer);
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

            App.KeblerControl.SelectedServer = ss.Tag as Server;
            App.KeblerControl.ReconnectToNewServer();
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
            App.KeblerControl.ShowConnectionManager();
        }

        private void ConnectFirst(object sender, RoutedEventArgs e)
        {
            App.KeblerControl.InitConnection();
        }

        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            App.KeblerControl.AddTorrent();
        }

        private void SortValMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //if (!(sender is MenuItem mi)) return;

            //ConfigService.Instanse.SortVal = mi.Tag.ToString();
            //ConfigService.Save();
            //App.KeblerControl.UpdateSorting();
        }

        private void SortTypeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //if (!(sender is MenuItem mi)) return;

            //int.TryParse(mi.Tag.ToString(), out var val);

            //ConfigService.Instanse.SortType = val;

            //ConfigService.Save();

            //App.KeblerControl.UpdateSorting();
        }

        private void Report(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/JeremiSharkboy/Kebler/issues") { CreateNoWindow = true });
        }

        private void About(object sender, RoutedEventArgs e)
        {
            var dd = new MessageBox(new About());
            dd.Owner = App.Instance.MainWindow;
            dd.ShowDialog();
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
