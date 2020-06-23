using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Kebler.Models;
using Kebler.Services;
using Kebler.Views;
using log4net;
using log4net.Config;

namespace Kebler
{
    /// <inheritdoc />
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static readonly ILog Log = LogManager.GetLogger(typeof(App));
        //public KeblerView KeblerControl;
        public static App Instance;

        public delegate void ConnectionToServerInitialisedHandler(Server srv);
        public static event ConnectionToServerInitialisedHandler ConnectionChanged;
        public delegate void ServerListChangedHandler();
        public static event ServerListChangedHandler ServerListChanged;



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

        public void Check(bool report = false)
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
            Instance = this;
            Check();

            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));


            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;



            //log4net.Config.XmlConfigurator.Configure(null);


            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            if (Current.Dispatcher != null)
                Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            ConfigService.LoadConfig();

            if (string.IsNullOrEmpty(ConfigService.Instanse.Language.Name))
            {
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList[0];
            }
            else
            {
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList.First(x => x.TwoLetterISOLanguageName == ConfigService.Instanse.Language.TwoLetterISOLanguageName);
            }
            SetEnv();
            InitializeComponent();

        }

        private void SetEnv()
        {

            {
#if DEBUG
                var glb = Environment.GetEnvironmentVariable(nameof(Kebler)+ "_DEBUG", EnvironmentVariableTarget.User);
                if (glb == null)
                    Environment.SetEnvironmentVariable(nameof(Kebler)+"_DEBUG", System.Reflection.Assembly.GetExecutingAssembly().Location, EnvironmentVariableTarget.User);
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


        //protected override void OnStartup(StartupEventArgs e)
        //{

        //    var control  = new KeblerView();
        //    control.Show();
        //    base.OnStartup(e);
        //}

        #endregion

    }
}
