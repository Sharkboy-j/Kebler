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
                var curretn = Assembly.GetExecutingAssembly().GetName().Version;
                var server = GetServerVersion();

                var result = curretn < server.Key;
                App.Instance.IsUpdateReady = result;
                if (result)
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



        public static void InstallUpdates()
        {
            if (File.Exists(Const.ConstStrings.InstallerExePath))
            {
                Process.Start(Const.ConstStrings.InstallerExePath);
                Application.Current.Shutdown(0);
            }
            else
            {
                var mgr = new WindowManager();
                mgr.ShowDialogAsync(new MessageBoxViewModel($"{Const.ConstStrings.InstallerExeName} not found. Try redownload app", string.Empty, Models.Enums.MessageBoxDilogButtons.Ok, true));
            }

        }


        static KeyValuePair<Version, Uri> GetServerVersion()
        {
            var pattern = string.Concat(Regex.Escape(Const.ConstStrings.GitHubRepo), Const.ConstStrings.GithubRegex);

            var urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var wrq = WebRequest.Create(string.Concat("https://github.com", Const.ConstStrings.GitHubRepo, "/releases/latest"));
            var wrs = wrq.GetResponse();

            using var sr = new StreamReader(wrs.GetResponseStream());
            string line;
            while (null != (line = sr.ReadLine()))
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                //if (!Uri.TryCreate(line, UriKind.Absolute, out var u))
                //    continue;


                var match = urlMatcher.Match(line);
                if (match.Success)
                {
                    var uri = new Uri(string.Concat("https://github.com", match.Value));


                    var vs = match.Value.LastIndexOf("/Release");
                    var sa = match.Value.Substring(vs + 9).Split('.', '/');
                    var v = new Version(int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
                    return new KeyValuePair<Version, Uri>(v, uri);
                }
            }

            return new KeyValuePair<Version, Uri>(null, null);
        }
    }
}
