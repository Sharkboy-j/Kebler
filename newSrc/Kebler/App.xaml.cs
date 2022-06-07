using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Kebler.Domain;
using Kebler.Services;
using Kebler.UI;
using Kebler.Views;

namespace Kebler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Bootstrapper _boots;
        private static Mutex _mutex;
        private const string ApplicationSingleInstanceMutextId = "KeblerInstance";
        public List<string> TorrentsToAdd = new();
        private const string AppcenterGuid = "b4abb5a6-9873-466e-97af-2b3879c23e42";
        public static App Instance;

        public App()
        {
            Instance = this;

            ConfigService.Instance.LoadConfig();

#if DEBUG
            if (Debugger.IsAttached)
            {
                ConfigService.DefaultSettingsInstanse.TraceEnabled = true;
            }
#endif

            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            Logger.Instance.Trace($"{nameof(Kebler)} app start");

            var args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
            {
                var fileInfo = new FileInfo(arg);
                if (fileInfo.Extension.Equals(ConstStrings.TorrentExt))
                    TorrentsToAdd.Add(fileInfo.FullName);
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            if (Current.Dispatcher != null)
                Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            _boots = new Bootstrapper();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Logger.Instance.Error(exception);
            }
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error(e.Exception);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
//#if RELEASE
//            Microsoft.AppCenter.AppCenter.Start(AppcenterGuid, typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Microsoft.AppCenter.Crashes.Crashes));
//            Microsoft.AppCenter.AppCenter.LogLevel = Microsoft.AppCenter.LogLevel.Verbose;
//#endif
//            _ = await Microsoft.AppCenter.AppCenter.IsEnabledAsync();


            var isFirstInstance = IsSingleInstance();
            if (!isFirstInstance)
            {
                Logger.Instance.Info($"{nameof(Kebler)} instance is already running.");
                Current.Shutdown(0);

                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex.Close();
            base.OnExit(e);
        }

        private static bool IsSingleInstance()
        {
            _mutex = new Mutex(false, ApplicationSingleInstanceMutextId);

            GC.KeepAlive(obj: _mutex);

            try
            {
                return _mutex.WaitOne(0, false);
            }
            catch (AbandonedMutexException)
            {
                _mutex.ReleaseMutex();
                return _mutex.WaitOne(0, false);
            }
        }
    }
}
