using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Kebler.Dialogs;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
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
            { CreateNoWindow = true });
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
            Process.Start(new ProcessStartInfo("cmd", "/c start https://t.me/jeremiSharkboy") { CreateNoWindow = true });
        }

        private void OpenLogs(object sender, RoutedEventArgs e)
        {
            try
            {
                var rootAppender = ((Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly()))
                    .Root.Appenders.OfType<FileAppender>().FirstOrDefault();

                var filename = rootAppender != null ? rootAppender.File : string.Empty;

                var filein = new FileInfo(filename);

                Process.Start(new ProcessStartInfo("explorer.exe", $"{filein.DirectoryName}") { CreateNoWindow = true });
            }
            catch (Exception ex)
            {
#if RELEASE

//                System.IO
//Path.GetFullPath(String path)
//System.IO.FileInfo
//System.IO.FileInfo..ctor(String originalPath, String fullPath, String fileName, Boolean isNormalized)
//System.IO.FileInfo
//System.IO.FileInfo..ctor(String fileName)
//Kebler.Views
//TopBarView.OpenLogs(Object sender, RoutedEventArgs e)
//System.Windows
//RoutedEventHandlerInfo.InvokeHandler(Object target, RoutedEventArgs routedEventArgs)
//System.Windows
//EventRoute.InvokeHandlersImpl(Object source, RoutedEventArgs args, Boolean reRaised)
//System.Windows
//UIElement.RaiseEventImpl(DependencyObject sender, RoutedEventArgs args)
//System.Windows
//UIElement.RaiseEvent(RoutedEventArgs e)
//System.Windows.Controls
//MenuItem.InvokeClickAfterRender(Object arg)
//System.Windows.Threading
//ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
//System.Windows.Threading
//ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)

                Crashes.TrackError(ex);
#endif
            }
        }
    }
}