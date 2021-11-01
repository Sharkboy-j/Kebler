using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Kebler.Services;
using Kebler.Services.Interfaces;
using Kebler.SI;
using Kebler.ViewModels;

namespace Kebler
{
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingle
    {
        public delegate void Langhandler();

        public static readonly ILog Log = Kebler.Services.Log.Instance;
        public static App Instance;

        public bool IsUpdateReady;
        public KeblerViewModel KeblerVm;
        private TaskbarIcon _notifyIcon;
        public List<string> TorrentsToAdd = new();
        private const string AppcenterGuid = "b4abb5a6-9873-466e-97af-2b3879c23e42";


        private App()
        {
            Instance = this;

            ConfigService.LoadConfig();
            SetEnv();


            var args = Environment.GetCommandLineArgs();

            FileAssociations.EnsureAssociationsSet();


            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(".torrent")) TorrentsToAdd.Add(fileInfo.FullName);
            }


            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;


            //log4net.Config.XmlConfigurator.Configure(null);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            if (Current.Dispatcher != null)
                Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;




            InitializeComponent();
        }


        private static void SetEnv()
        {
            var glb = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(nameof(Kebler), Process.GetCurrentProcess().MainModule.FileName,
                EnvironmentVariableTarget.User);
        }


        protected override void OnStartup(StartupEventArgs e)
        {
#if !DEBUG
            Microsoft.AppCenter.AppCenter.Start(AppcenterGuid, typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Microsoft.AppCenter.Crashes.Crashes));
            Microsoft.AppCenter.AppCenter.LogLevel = Microsoft.AppCenter.LogLevel.Verbose;
#endif


            var isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance(nameof(Kebler));
            if (!isFirstInstance) Current.Shutdown();
            base.OnStartup(e);

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose();
            SingleInstance<App>.Cleanup();
            base.OnExit(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (Current.MainWindow is Window wnd)
            {
                wnd.Activate();

                if (wnd?.WindowState != WindowState.Normal)
                    wnd.WindowState = WindowState.Normal;
            }

        }

        #region Events

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Error(exception);
            }
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception);
            e.Handled = true;
        }

        public void OnInstanceInvoked(string[] args)
        {
            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(".torrent") || arg.StartsWith("magnet")) TorrentsToAdd.Add(arg);
            }

            KeblerVm.OpenPaseedWithArgsFiles();
        }

        #endregion

        private void NI_ExitClick(object sender, RoutedEventArgs e)
        {
            KeblerVm.Exit();
        }
    }
}