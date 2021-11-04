namespace Kebler
{
#if RELEASE
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using Caliburn.Micro;
    using Const;
    using Models;
    using Resources;
    using Services;
    using Kebler.Update.Core;
    using ViewModels;
    using System.Threading.Tasks;
#endif

    internal static class Updater
    {
#if RELEASE

        private static readonly Kebler.Services.Interfaces.ILog Log = Kebler.Services.Log.Instance;

        public static async void CheckUpdates()
        {

            Log.Info("Start check updates");
            try
            {
                var current = Assembly.GetExecutingAssembly().GetName().Version;

                var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), current, ConfigService.Instanse.AllowPreRelease);

                Log.Info($"Current {current} Serv {result.Item2.name}");

                App.Instance.IsUpdateReady = result.Item1;
                if (result.Item1)
                {

                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        var mgr = new WindowManager();

                        var lt = LocalizationProvider.GetLocalizedValue(nameof(Strings.NewUpdate));

                        var dialogres = await mgr.ShowDialogAsync(new MessageBoxViewModel(lt.Replace("%d", result.Item2.tag_name), string.Empty,
                          Enums.MessageBoxDilogButtons.YesNo, true));
                        if (dialogres == true)
                            await Task.Run(InstallUpdates);
                    });
                }
            }
            catch (Exception ex)
            {
                App.Instance.IsUpdateReady = false;
                Log.Error(ex);
            }
        }

        public static void InstallUpdates()
        {
            Directory.CreateDirectory(ConstStrings.TempInstallerFolder);
            File.Copy(ConstStrings.InstallerExePath, ConstStrings.TempInstallerExePath, true);

            using (var process = new Process())
            {
                var info = new ProcessStartInfo
                {
                    FileName = ConstStrings.TempInstallerExePath,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                if (ConfigService.Instanse.AllowPreRelease)
                    info.Arguments = $"{ConstStrings.Args.Beta}";

                process.StartInfo = info;
                process.EnableRaisingEvents = false;
                process.Start();
            }

            Application.Current.Shutdown();
        }
#endif
    }
}
