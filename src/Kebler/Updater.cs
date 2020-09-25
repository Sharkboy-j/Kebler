using Caliburn.Micro;
using Kebler.UI.CSControls.MuliTreeView;
using Kebler.ViewModels;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ILog = log4net.ILog;
using LogManager = log4net.LogManager;
using Kebler.Services;

namespace Kebler
{
    static class Updater
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task CheckUpdates()
        {
            Log.Info("Check updates");
            try
            {
                var current = Assembly.GetExecutingAssembly().GetName().Version;
                var result = await Check(Const.ConstStrings.GITHUB_USER, nameof(Kebler), current);


                Log.Info($"Current {current} Serv {result.Item2}");

                App.Instance.IsUpdateReady = result.Item1;
                if (result.Item1)
                {
                    var mgr = new WindowManager();
                    var dialogres = await mgr.ShowDialogAsync(new MessageBoxViewModel("New update is ready. Install it?", string.Empty, Models.Enums.MessageBoxDilogButtons.YesNo, true));
                    if (dialogres == true)
                    {
                        InstallUpdates();
                    }
                }
            }
            catch (Exception ex)
            {
                App.Instance.IsUpdateReady = false;
                Log.Error(ex);
            }

        }


        static bool FileInUse(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    //seems okay
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static void InstallUpdates()
        {
            if (!FileInUse(Const.ConstStrings.InstallerExePath))
            {
                Directory.CreateDirectory(Const.ConstStrings.TempInstallerFolder);
                File.Copy(Const.ConstStrings.InstallerExePath, Const.ConstStrings.TempInstallerExePath,true);

                using (Process process = new Process())
                {
                    var info = new ProcessStartInfo();
                    info.FileName = Const.ConstStrings.TempInstallerExePath;
                    info.UseShellExecute = true;
                    info.CreateNoWindow = true;
                    info.RedirectStandardOutput = false;
                    info.RedirectStandardError = false;

                    process.StartInfo = info;
                    process.EnableRaisingEvents = false;
                    process.Start();
                }
                Application.Current.Shutdown();
            }
            else
            {
                var mgr = new WindowManager();
                mgr.ShowDialogAsync(new MessageBoxViewModel($"{Const.ConstStrings.InstallerExeName} not found. Try redownload app", string.Empty, Models.Enums.MessageBoxDilogButtons.Ok, true));
            }

        }

        public static async Task<(bool, Version)> Check(string user, string repository, Version currentVersion)
        {
            var gitHub = new GitHubApi();
            var latestReleaseJson = await gitHub.GetLatestReleaseJSONAsync(user, repository);
            var version = GitHubApi.ExtractVersion(latestReleaseJson);

            return (currentVersion < version, version);

        }
    }
}
