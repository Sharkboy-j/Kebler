using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Kebler.Dialogs;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using LogManager = log4net.LogManager;

namespace Kebler.Views
{
    /// <summary>
    ///     Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView
    {
        private IWindowManager manager = new WindowManager();

        public TopBarView()
        {
            InitializeComponent();
        }


        private void Report(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/JeremiSharkboy/Kebler/issues")
                {CreateNoWindow = true});
        }

        private void About(object sender, RoutedEventArgs e)
        {
            var dialog = new About(Application.Current.MainWindow);
            dialog.ShowDialog();
        }

        private async void Check(object sender, RoutedEventArgs e)
        {
            await Updater.CheckUpdates();
        }

        private void Contact(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://t.me/jeremiSharkboy") {CreateNoWindow = true});
        }

        private void OpenLogs(object sender, RoutedEventArgs e)
        {
            var rootAppender = ((Hierarchy) LogManager.GetRepository(Assembly.GetEntryAssembly()))
                .Root.Appenders.OfType<FileAppender>()
                .FirstOrDefault();
            var filename = rootAppender != null ? rootAppender.File : string.Empty;

            var filein = new FileInfo(filename);

            Process.Start(new ProcessStartInfo("explorer.exe", $"{filein.DirectoryName}") {CreateNoWindow = true});
        }
    }
}