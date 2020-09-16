using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using IWshRuntimeLibrary;

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public StringBuilder BUILDER = new StringBuilder();
        public static App Instance;

        App()
        {
            Instance = this;
            Log("Start App");

            foreach (var process in Process.GetProcessesByName(nameof(Kebler)))
            {
                process.Kill();
                Log("Killed kebler");
            }

            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName("Installer"))
            {
                if (process.Id == current.Id) continue;
                process.Kill();
                Log("Killed installer");
            }

            

            try
            {
                var module = Process.GetCurrentProcess()?.MainModule;
                var path = module?.FileName;



                Log($"Current Path: {Const.Strings.KeblerExepath}");

                if (path.Equals(Const.Strings.KeblerExepath))
                {
                    Log("Try start from Temp");
                    var temp = Path.GetTempFileName();
                    System.IO.File.Copy(Const.Strings.KeblerExepath, temp, true);
                    Process.Start(temp);
                    Log("Started Temp");
                    Current.Shutdown(0);
                }
                else
                {
                    Log("Go for Update");

                    Console.WriteLine("CheckUpdate");
                    HasUpdate();
                    Current.Shutdown(0);
                }
            }
            catch (Exception ex)
            {
                var ss = new StringBuilder();
                ss.Append(ex.Message);
                ss.Append(ex);
                ss.Append(ex.StackTrace);
                new EXCEPTIONWINDOW(ss.ToString()).ShowDialog();
                Current.Shutdown(0);
            }

        }


        public void Log(string msg)
        {
            BUILDER.Append(msg + Environment.NewLine);
        }

        static KeyValuePair<Version, Uri> GetVersion()
        {
            var pattern = string.Concat(Regex.Escape(Const.Strings.GitHubRepo),
                @"\/releases\/download\/*\/[0-9]+.[0-9]+.[0-9]+.[0-9].*\.zip");

            var urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var result = new Dictionary<Version, Uri>();
            var wrq = WebRequest.Create(string.Concat("https://github.com", Const.Strings.GitHubRepo, "/releases/latest"));
            var wrs = wrq.GetResponse();

            using var sr = new StreamReader(wrs.GetResponseStream());
            string line;
            while (null != (line = sr.ReadLine()))
            {
                var match = urlMatcher.Match(line);
                if (match.Success)
                {
                    var uri = new Uri(string.Concat("https://github.com", match.Value));


                    var vs = match.Value.LastIndexOf("/Release", StringComparison.Ordinal);
                    var sa = match.Value.Substring(vs + 9).Split('.', '/');
                    var v = new Version(int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
                    return new KeyValuePair<Version, Uri>(v, uri);
                }
            }

            return new KeyValuePair<Version, Uri>(null, null);
        }

        public void HasUpdate()
        {
            var latest = GetVersion();
            Log($"Server: {latest.Key}|{latest.Value}");

            var getEnv = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);
            Log($"Env Path: {getEnv}");
            if (getEnv == null)
            {
                var wd = new MainWindow(latest.Value);
                wd.ShowDialog();
                Current.Shutdown(0);
            }
            else
            {

                if (System.IO.File.Exists(Const.Strings.KeblerExepath))
                {
                    var v = new Version(FileVersionInfo.GetVersionInfo(getEnv).FileVersion);
                    if (latest.Key > v)
                    {
                        var wd = new MainWindow(latest.Value);
                        wd.ShowDialog();
                        Current.Shutdown(0);
                    }
                    else
                    {
                        CreateShortcut();
                        Process.Start(getEnv);
                        Current.Shutdown(0);
                    }
                }
                else
                {
                    var wd = new MainWindow(latest.Value);
                    wd.ShowDialog();
                    Current.Shutdown(0);
                }
            }
        }

        public void CreateShortcut()
        {
            object shDesktop = "Desktop";
            var shell = new WshShell();
            var shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Kebler.lnk";
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Kebler";
            shortcut.TargetPath = Const.Strings.KeblerExepath;
            shortcut.Save();
        }
    }
}
