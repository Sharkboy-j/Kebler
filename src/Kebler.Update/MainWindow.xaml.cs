using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Windows;
using Kebler.Const;
using Kebler.Update.Core;

namespace Kebler.Update
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private WebClient _webClient;
        private string tempfile;
        new bool DialogResult;

        public MainWindow()
        {

            try
            {

                var current = Process.GetCurrentProcess();

                KillAllInstancesExceptThis(current);

                var module = current?.MainModule;
                var curretnPath = module?.FileName;
                Log($"Current Path: {curretnPath}");

                if (string.IsNullOrEmpty(curretnPath))
                {
                    Log($"Current Path is null. WTF?!");
                    App.DONE(false);
                    return;
                }

                // we have to start installer from any folder but not ConstStrings.InstallerExePath. so let check it
                if (curretnPath.Equals(ConstStrings.InstallerExePath))
                {
                    MoveToTempStartAndExit();
                }
                else
                {
                    Log($"Go for Update from {curretnPath}");
                    Console.WriteLine("CheckUpdate");
                    InitializeComponent();
                    Loaded += MainWindow_Loaded;
                }
            }
            catch (Exception ex)
            {
                var ss = new StringBuilder();
                ss.Append(ex.Message);
                ss.Append(ex);
                ss.Append(ex.StackTrace);
                Log(ss.ToString());
                App.DONE(false);

            }


        }

        private void MoveToTempStartAndExit()
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

            Log("Started from Temp");
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private static void KillAllInstancesExceptThis(Process current)
        {
            //kill Kebler
            foreach (var process in Process.GetProcessesByName(nameof(Kebler)))
            {
                process.Kill(false);
            }

            //Kill Kebler updater
            Process[] p = Process.GetProcessesByName(ConstStrings.InstallerName);
            foreach (var pro in p)
            {
                if (pro.Id != current.Id)
                {
                    pro.Kill();
                }
            }
        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Force)
            {
                startFree();
            }
            else
            {
                string? getEnv = null;
                Version? current = null;
                getEnv = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);

                if (!string.IsNullOrEmpty(getEnv))
                {
                    Log($"We found old version on GetEnvironmentVariable: {getEnv}");
                    if (File.Exists(getEnv))
                    {
                        current = new Version(FileVersionInfo.GetVersionInfo(getEnv).FileVersion);
                        Log($"Current version is: {current}");

                        Log($"Okay. Try get server version (github version)");
                        var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), current, App.Beta);

                        Log($"Server version is: {result.Item2.name}");

                        if (result.Item2.name > current)
                        {
                            var updateUrl = result.Item2.assets.Last().browser_download_url;
                            Log($"So here is: {updateUrl}");

                            StartDownlaod(result.Item2.assets.Last().browser_download_url);
                        }
                        else
                        {
                            Process.Start(getEnv);
                            App.DONE(true);
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






        }

        async void startFree()
        {
            Log($"Oh my god. That is first time..... go for update with 0.0.0.0 version");
            var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), new Version(0, 0, 0, 0), App.Beta);

            App.CreateShortcut();
            var updateUrl = result.Item2.assets.LastOrDefault()?.browser_download_url;
            StartDownlaod(updateUrl);
        }

        private void StartDownlaod(string? uri)
        {

            if (string.IsNullOrEmpty(uri))
            {
                var resp = MessageBox.Show("Error url");
                CustomizableWindow_Closing(null, null);
                return;
            }

            _webClient = new WebClient();
            tempfile = Path.GetTempFileName();

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted += _webClient_DownloadFileCompleted;
            //Size.Content = "Downlaoding...";
            _webClient.DownloadFileAsync(new Uri(uri), tempfile);
        }

        private void _webClient_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    Close();
                    App.DONE(false);
                }


                App.Log("Download completed");

                ContentDisposition contentDisposition = null;
                if (_webClient.ResponseHeaders?["Content-Disposition"] != null)
                    contentDisposition = new ContentDisposition(_webClient.ResponseHeaders["Content-Disposition"]);

                var fileName = contentDisposition.FileName;

                var pth = Path.Combine(Path.GetTempPath(), fileName);
                if (File.Exists(pth)) File.Delete(pth);
                File.Move(tempfile, pth);

                var zip = new ZipArchive(new FileStream(pth, FileMode.Open));
                zip.ExtractToDirectory(ConstStrings.KeblerRoamingFolder, true);
                DialogResult = true;
                App.DONE(true);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());

                App.DONE(false);
            }
        }

        void Log(string msg)
        {
            App.Log(msg);
        }



        public static void DeleteDirectory(string path)
        {
            foreach (var directory in Directory.GetDirectories(path)) DeleteDirectory(directory);

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //Size.Content = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            PB.Value = e.ProgressPercentage * 100;
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

        private void CustomizableWindow_Closing(object? sender, CancelEventArgs? e)
        {


            if (_webClient is not null)
            {
                _webClient.CancelAsync();
                _webClient.DownloadProgressChanged -= OnDownloadProgressChanged;
                _webClient.DownloadFileCompleted -= _webClient_DownloadFileCompleted;
                _webClient.Dispose();
            }




            App.DONE(DialogResult);
        }

        private void Button_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CustomizableWindow_Closing(null, null);
        }
    }
}