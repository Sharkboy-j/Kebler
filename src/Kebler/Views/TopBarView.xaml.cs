using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Kebler.Dialogs;
using Kebler.Services;

namespace Kebler.Views
{
    /// <summary>
    ///     Interaction logic for TopBarView.xaml
    /// </summary>
    public partial class TopBarView
    {

        public TopBarView()
        {
            InitializeComponent();
        }


        private void Report(object sender, RoutedEventArgs e)
        {
            App.Log.Ui(nameof(Report));
            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/Rebell81/Kebler/issues")
            { CreateNoWindow = true });
        }

        private void About(object sender, RoutedEventArgs e)
        {
            App.Log.Ui(nameof(About));

            var dialog = new About(Application.Current.MainWindow);
            dialog.ShowDialog();
        }

        private void Check(object sender, RoutedEventArgs e)
        {
#if RELEASE
            App.Log.Ui(nameof(Check));

            Task.Run(Updater.CheckUpdates);
#endif
        }

        private void Contact(object sender, RoutedEventArgs e)
        {
            App.Log.Ui(nameof(Contact));

            Process.Start(new ProcessStartInfo("cmd", "/c start https://github.com/Rebell81") { CreateNoWindow = true });
        }

        private void OpenLogs(object sender, RoutedEventArgs e)
        {
            App.Log.Ui(nameof(OpenLogs));

            try
            {
                var args = $@"/c start ""{Log.LogFileInfo.DirectoryName}""";
                Process.Start(new ProcessStartInfo("explorer.exe", args) { CreateNoWindow = true });
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

                App.Log.Error(ex);
#endif
            }
        }
    }
}