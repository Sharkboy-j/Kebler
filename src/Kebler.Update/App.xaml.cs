using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
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

                Log($"Current Path: {path}");

                if (path.Equals(Const.ConstStrings.InstallerExePath))
                {
                    Log("Try start from Temp");
                    var temp = Path.GetTempFileName();
                    System.IO.File.Copy(Const.ConstStrings.InstallerExePath, temp, true);
                    Process.Start(temp);
                    Log("Started Temp");
                    Current.Shutdown(0);
                }
                else
                {
                    Log($"Go for Update from {path}");

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
            var pattern = string.Concat(Regex.Escape(Const.ConstStrings.GitHubRepo),
                @"\/releases\/download\/*\/[0-9]+.[0-9]+.[0-9]+.[0-9].*\.zip");

            var urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var result = new Dictionary<Version, Uri>();
            var wrq = WebRequest.Create(string.Concat("https://github.com", Const.ConstStrings.GitHubRepo,
                "/releases/latest"));
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

                if (System.IO.File.Exists(Const.ConstStrings.KeblerExepath))
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
            var lnkFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Kebler.lnk");
            Shortcut.Create(lnkFileName, Const.ConstStrings.KeblerExepath,
                null, null, "Kebler", null, null);
        }

    }

    public class Shortcut
    {
        public static void Create(string fileName, string targetPath, string arguments, string workingDirectory, string description, string hotkey, string iconPath)
        {
            IWshShortcut shortcut = (IWshShortcut)m_type.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, m_shell, new object[] { fileName });
            shortcut.Description = description;
            shortcut.TargetPath = targetPath;
            if (!string.IsNullOrEmpty(iconPath))
                shortcut.IconLocation = iconPath;
            shortcut.Save();
        }

        private static Type m_type = Type.GetTypeFromProgID("WScript.Shell");
        private static object m_shell = Activator.CreateInstance(m_type);

        [ComImport, TypeLibType((short)0x1040), Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
        private interface IWshShortcut
        {
            [DispId(0)]
            string FullName
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0)]
                get;
            }

            [DispId(0x3e8)]
            string Arguments
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e8)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e8)]
                set;
            }

            [DispId(0x3e9)]
            string Description
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e9)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3e9)]
                set;
            }

            [DispId(0x3ea)]
            string Hotkey
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ea)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ea)]
                set;
            }

            [DispId(0x3eb)]
            string IconLocation
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3eb)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3eb)]
                set;
            }

            [DispId(0x3ec)]
            string RelativePath
            {
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ec)]
                set;
            }

            [DispId(0x3ed)]
            string TargetPath
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ed)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ed)]
                set;
            }

            [DispId(0x3ee)] int WindowStyle { [DispId(0x3ee)] get; [param: In] [DispId(0x3ee)] set; }

            [DispId(0x3ef)]
            string WorkingDirectory
            {
                [return: MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ef)]
                get;
                [param: In, MarshalAs(UnmanagedType.BStr)]
                [DispId(0x3ef)]
                set;
            }

            [TypeLibFunc((short)0x40), DispId(0x7d0)]
            void Load([In, MarshalAs(UnmanagedType.BStr)] string PathLink);

            [DispId(0x7d1)]
            void Save();
        }
    }
}
