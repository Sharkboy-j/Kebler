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
using AutoUpdaterDotNET;
using FamFamFam.Flags.Wpf;
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
        public static readonly ILog Log = LogManager.GetLogger(typeof(App));
        public KeblerViewModel KeblerVM;
        public static App Instance;
        public GeoService Geo;
        public delegate void ConnectionToServerInitialisedHandler(Server srv);
        public static event ConnectionToServerInitialisedHandler ConnectionChanged;
        public delegate void ServerListChangedHandler();
        public static event ServerListChangedHandler ServerListChanged;
        public CountryIdToFlagImageSourceConverter Flags = new CountryIdToFlagImageSourceConverter();
        public List<string> torrentsToAdd = new List<string>();

        public delegate void Langhandler();
        public event Langhandler LangChanged;

        public static void ChangeConnectedServer(Server srv)
        {
            ConnectionChanged?.Invoke(srv);
        }

        public static void InvokeServerListChanged()
        {
            ServerListChanged?.Invoke();
        }

        public void LangChangedNotify()
        {
            LangChanged?.Invoke();
        }

        public object FindRes(string name)
        {
            return FindResource(name);
        }

        public void CheckUpdates(bool report = false)
        {
            Log.Info("AutoUpdater.Start");
            AutoUpdater.NotifyInfoArgsEvent += NotifyInfoArgsEvent;

            AutoUpdater.ReportErrors = report;
            AutoUpdater.DownloadPath = Path.GetTempPath();
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.RunUpdateAsAdmin = true;

            Dispatcher.Invoke(() =>
            {
                AutoUpdater.Start("https://raw.githubusercontent.com/JeremiSharkboy/Kebler/master/version.xml");
            });
        }

        App()
        {
            var args = Environment.GetCommandLineArgs();

            FileAssociations.EnsureAssociationsSet();
            var builder = new GeoServiceBuilder();
            builder.RegisterResource<EmbeddedResourceReader>();

            Geo = new GeoService(builder);

            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(".torrent"))
                {
                    torrentsToAdd.Add(fileInfo.FullName);
                }
            }

            Instance = this;
            CheckUpdates();

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

        private void SetEnv()
        {

            {
#if DEBUG
                var glb = Environment.GetEnvironmentVariable(nameof(Kebler) + "_DEBUG", EnvironmentVariableTarget.User);
                if (glb == null)
                    Environment.SetEnvironmentVariable(nameof(Kebler) + "_DEBUG", System.Reflection.Assembly.GetExecutingAssembly().Location, EnvironmentVariableTarget.User);
#else
                var glb = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);
                if (glb == null)
                    Environment.SetEnvironmentVariable(nameof(Kebler), System.Reflection.Assembly.GetExecutingAssembly().Location, EnvironmentVariableTarget.User);

#endif
            }

        }


        private void NotifyInfoArgsEvent(UpdateInfoEventArgs args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fileVersionInfo.FileVersion;

            Log.Info($"Installed {version} | Current {args.CurrentVersion}");
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

            if (torrentsToAdd.Count > 0)
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SingleInstance<App>.Cleanup();
            base.OnExit(e);
        }

    }



}
