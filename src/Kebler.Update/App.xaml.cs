using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string GitHubRepo = "/JeremiSharkboy/Kebler";

        App()
        {
            try
            {
                var module = Process.GetCurrentProcess()?.MainModule;
                var path = module?.FileName;

                var check = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    nameof(Kebler), module?.ModuleName);
                if (path.Equals(check))
                {
                    var temp = Path.GetTempFileName();
                    File.Copy(check, temp, true);
                    Process.Start(temp);
                    Console.WriteLine("OKEY");
                    Current.Shutdown(0);
                }
                else
                {
                    Console.WriteLine("CheckUpdate");
                    HasUpdate();
                    Current.Shutdown(0);
                }
            }
            catch(Exception ex)
            {
                var ss = new StringBuilder();
                ss.Append(ex.Message);
                ss.Append(ex);
                ss.Append(ex.StackTrace);
                new EXCEPTIONWINDOW(ss.ToString()).ShowDialog();
                Current.Shutdown(0);
            }
           
        }


   

        static KeyValuePair<Version, Uri> GetVersion()
        {
            var pattern = string.Concat(Regex.Escape(GitHubRepo),
                @"\/releases\/download\/*\/[0-9]+.[0-9]+.[0-9]+.[0-9].*\.zip");

            var urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var result = new Dictionary<Version, Uri>();
            var wrq = WebRequest.Create(string.Concat("https://github.com", GitHubRepo, "/releases/latest"));
            var wrs = wrq.GetResponse();

            using var sr = new StreamReader(wrs.GetResponseStream());
            string line;
            while (null != (line = sr.ReadLine()))
            {
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

        public static void HasUpdate()
        {
            var latest = GetVersion();
            var getEnv = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);

            if (getEnv == null)
            {
                var wd = new MainWindow(latest.Value);
                wd.ShowDialog();
                Current.Shutdown(0);
            }
            else
            {
                var keblerexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    nameof(Kebler), nameof(App), $"{nameof(Kebler)}.exe");
                if (File.Exists(keblerexe))
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
    }
}
