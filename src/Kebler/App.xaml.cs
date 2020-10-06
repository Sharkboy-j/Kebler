using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using Kebler.Services;
using Kebler.SI;
using Kebler.ViewModels;
using log4net.Config;
using ILog = log4net.ILog;
using LogManager = log4net.LogManager;

namespace Kebler
{
    /// <inheritdoc />
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingle
    {
        public delegate void Langhandler();

        public static readonly ILog Log = LogManager.GetLogger(typeof(App));
        public static App Instance;

        public bool IsUpdateReady;
        public KeblerViewModel KeblerVM;
        private TaskbarIcon notifyIcon;
        public List<string> torrentsToAdd = new List<string>();


        private App()
        {
            var args = Environment.GetCommandLineArgs();

            FileAssociations.EnsureAssociationsSet();


            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(".torrent")) torrentsToAdd.Add(fileInfo.FullName);
            }

            Instance = this;

            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));


            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;


            //log4net.Config.XmlConfigurator.Configure(null);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            if (Current.Dispatcher != null)
                Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            ConfigService.LoadConfig();


            SetEnv();
            InitializeComponent();
        }

        public object FindRes(string name)
        {
            return FindResource(name);
        }

        private void SetEnv()
        {
            var glb = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(nameof(Kebler), Process.GetCurrentProcess().MainModule.FileName,
                EnvironmentVariableTarget.User);
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            var isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance(nameof(Kebler));
            if (!isFirstInstance) Current.Shutdown();
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose();
            SingleInstance<App>.Cleanup();
            base.OnExit(e);
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var dd = IoC.Get<KeblerViewModel>();

            dd.State = WindowState.Normal;
        }


        #region Events

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!(e.ExceptionObject is Exception exception)) return;
            Log.Error(exception);
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
                if (fileInfo.Extension.Equals(".torrent") || arg.StartsWith("magnet")) torrentsToAdd.Add(arg);
            }


            KeblerVM.OpenPaseedWithArgsFiles();
        }

        #endregion
    }
}