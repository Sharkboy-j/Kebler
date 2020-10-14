using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Kebler.Const;
using Kebler.Services;

namespace Kebler.Update
{
    public partial class App : Application
    {
        public static StringBuilder BUILDER = new StringBuilder();

        private App()
        {
            try
            {
                var module = Process.GetCurrentProcess()?.MainModule;
                var path = module?.FileName;

                Log($"Current Path: {path}");

                if (path.Equals(ConstStrings.InstallerExePath))
                {
                    Log("Try start from Temp");
                    Directory.CreateDirectory(ConstStrings.TempInstallerFolder);
                    File.Copy(ConstStrings.InstallerExePath, ConstStrings.TempInstallerExePath, true);

                    using (var process = new Process())
                    {
                        var info = new ProcessStartInfo
                        {
                            FileName = ConstStrings.TempInstallerExePath,
                            UseShellExecute = true,
                            CreateNoWindow = true
                        };

                        process.StartInfo = info;
                        process.EnableRaisingEvents = false;
                        process.Start();
                    }

                    Log("Started Temp");
                    Current.Shutdown();
                    Environment.Exit(0);
                    return;
                }
                else
                {
                    Log($"Go for Update from {path}");

                    Console.WriteLine("CheckUpdate");
                    HasUpdate();
                }
            }
            catch (Exception ex)
            {
                var ss = new StringBuilder();
                ss.Append(ex.Message);
                ss.Append(ex);
                ss.Append(ex.StackTrace);
                Log($"{ss}");
                File.AppendAllText("install.log", BUILDER.ToString());
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        public static void Log(string msg)
        {
            BUILDER.Append(msg + Environment.NewLine);
        }

        public void HasUpdate()
        {
            string? getEnv = null;
            Version? current = null;

            //try find installed version
            getEnv = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);

            if (!string.IsNullOrEmpty(getEnv))
            {
                Log($"We found old version on: {getEnv}");

                if (File.Exists(getEnv))
                {
                    current = new Version(FileVersionInfo.GetVersionInfo(getEnv).FileVersion);
                    Log($"Current version is: {current}");

                    Log($"Okay. Try get server version (github version)");
                    var result = UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), current);
                    Log($"Server version is: {result.Item2}");

                    if (result.Item2 > current)
                    {
                        Log($"So we have old version....");

                        Log("Try get server version uri");
                        var updateUrl = UpdaterApi.GetlatestUri();
                        Log($"So here is: {updateUrl}");

                        var wd = new MainWindow(new Uri(updateUrl));
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
                    startFree();

                }
            }
            else
            {
                startFree();
            }
        }

        void startFree()
        {
            Log($"Oh my god. That is first time..... go for update with 0.0.0.0 version");
            var result = UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), new Version(0, 0, 0, 0));
            var updateUrl = UpdaterApi.GetlatestUri();

            var wd = new MainWindow(new Uri(updateUrl));
            if (wd.ShowDialog() == true)
            {
                CreateShortcut();
                startKebler();
            }
            Application.Current.Shutdown();

        }

        void startKebler()
        {
            Process.Start(new ProcessStartInfo()
            {
                WorkingDirectory = Const.ConstStrings.KeblerAppFolderPath,
                FileName = ConstStrings.KeblerExepath,
                Arguments = Const.ConstStrings.KeblerAppFolderPath,
                UseShellExecute = true,
            });
        }

        public static void CreateShortcut()
        {
            var lnkFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{nameof(Kebler)}.lnk");
            Shortcut.Create(lnkFileName, ConstStrings.KeblerExepath,
                ConstStrings.KeblerAppFolderPath, ConstStrings.KeblerAppFolderPath, "", null, ConstStrings.KeblerExepath);
        }
    }
}