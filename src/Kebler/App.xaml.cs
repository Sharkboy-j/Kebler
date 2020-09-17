using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Caliburn.Micro;
using FamFamFam.Flags.Wpf;
using Hardcodet.Wpf.TaskbarNotification;
using Kebler.Models;
using Kebler.Services;
using Kebler.SI;
using Kebler.ViewModels;
using log4net;
using log4net.Config;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Plarium.Geo;
using Plarium.Geo.Embedded;
using Plarium.Geo.Services;


namespace Kebler
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ISingle
    {
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(App));
        public KeblerViewModel KeblerVM;
        public static App Instance;
        public List<string> torrentsToAdd = new List<string>();
        private TaskbarIcon notifyIcon;

        public delegate void Langhandler();
        public bool IsUpdateReady;

        public object FindRes(string name)
        {
            return FindResource(name);
        }


        App()
        {
            var args = Environment.GetCommandLineArgs();

            FileAssociations.EnsureAssociationsSet();
      

            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(".torrent"))
                {
                    torrentsToAdd.Add(fileInfo.FullName);
                }
            }

            Instance = this;

            var logRepo = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
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

        private void SetEnv()
        {
            var glb = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(nameof(Kebler), Process.GetCurrentProcess().MainModule.FileName, EnvironmentVariableTarget.User);
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
                if (fileInfo.Extension.Equals(".torrent") || arg.StartsWith("magnet"))
                {
                    torrentsToAdd.Add(arg);
                }

            }


            KeblerVM.OpenPaseedWithArgsFiles();
        }
        #endregion


        protected override void OnStartup(StartupEventArgs e)
        {
            var isFirstInstance = SingleInstance<App>.InitializeAsFirstInstance(nameof(Kebler));
            if (!isFirstInstance)
            {
                Current.Shutdown();
            }
            base.OnStartup(e);

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

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
    }



}
