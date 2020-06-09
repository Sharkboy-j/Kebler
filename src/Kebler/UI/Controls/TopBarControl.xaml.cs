using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models;
using Kebler.Services;
using Transmission.API.RPC.Entity;

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
            foreach (var item in LocalizationManager.CultureList)
            {
                var dd = new MenuItem();
                dd.Header = item.EnglishName;
                dd.Tag = item;
                dd.Click += Dd_Click;
                dd.IsCheckable = true;

                //ahhh fuck this shit. im too lasy for.. That is fucking easiest and fastes way. stfu!
                //TODO: RMK
                //dd.Style = App.Instance.TryFindResource("PopUpTopBarMenuButton") as Style;


                dd.IsChecked = Thread.CurrentThread.CurrentCulture == item;

                LangMenu.Items.Add(dd);
            }
        }

        private void Dd_Click(object sender, RoutedEventArgs e)
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

        private void SortValMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem mi)) return;

            ConfigService.Instanse.SortVal = mi.Tag.ToString();
            ConfigService.Save();
            App.KeblerControl.UpdateSorting();
        }

        private void SortTypeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem mi)) return;

            int.TryParse(mi.Tag.ToString(), out var val);

            ConfigService.Instanse.SortType = val;

            ConfigService.Save();

            App.KeblerControl.UpdateSorting();
        }

        private void Report(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/JeremiSharkboy/Kebler/issues") { CreateNoWindow = true });
        }

        private void About(object sender, RoutedEventArgs e)
        {
            var dd = new Windows.MessageBox(new About());
            dd.ShowDialog(Application.Current.MainWindow);
        }

        private void Check(object sender, RoutedEventArgs e)
        {
            App.Instance.Check(true);
        }

        private void Contact(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start https://t.me/jeremiSharkboy") { CreateNoWindow = true });
        }
    }
}
