using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Services;
using Kebler.ViewModels;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using LogManager = log4net.LogManager;
using MessageBox = Kebler.Dialogs.MessageBox;

namespace Kebler.UI
{
    /// <summary>
    /// Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView
    {
        IWindowManager manager = new WindowManager();
        public TopBarView()
        {
            InitializeComponent();

            //foreach (var item in LocalizationManager.CultureList)
            //{
            //    var dd = new MenuItem { Header = item.EnglishName, Tag = item };
            //    dd.Click += langChanged;
            //    dd.IsCheckable = true;
            //    dd.IsChecked = Equals(Thread.CurrentThread.CurrentCulture, item);
            //    LangMenu.Items.Add(dd);
            //}


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
            manager.ShowDialogAsync(new ConnectionManagerViewModel());
        }

        private void ConnectFirst(object sender, RoutedEventArgs e)
        {
            //App.Instance.KeblerControl.InitConnection();
        }

        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            //App.Instance.KeblerControl.AddTorrent();
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

        private void OpenLogs(object sender, RoutedEventArgs e)
        {
            var rootAppender = ((Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly()))
                .Root.Appenders.OfType<FileAppender>()
                .FirstOrDefault();
            string filename = rootAppender != null ? rootAppender.File : string.Empty;

            var filein = new FileInfo(filename);

            Process.Start(new ProcessStartInfo("explorer.exe", $"{filein.DirectoryName}") { CreateNoWindow = true });

        }
    }
}
