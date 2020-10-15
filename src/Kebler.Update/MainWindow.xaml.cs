using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using Kebler.Const;
using Kebler.Services;

namespace Kebler.Update
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private WebClient _webClient;
        private string tempfile;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }





        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
                    var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), current);

                    Log($"Server version is: {result.Item2.name}");

                    if (result.Item2.name > current)
                    {
                        var updateUrl = result.Item2.assets.LastOrDefault().browser_download_url;
                        Log($"So here is: {updateUrl}");

                        StartDownlaod(result.Item2.assets.LastOrDefault().browser_download_url);
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

        async void startFree()
        {
            Log($"Oh my god. That is first time..... go for update with 0.0.0.0 version");
            var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), new Version(0, 0, 0, 0),true);
            var updateUrl = result.Item2.assets.LastOrDefault().browser_download_url;
            StartDownlaod(updateUrl);
        }

        private void StartDownlaod(string uri)
        {
            _webClient = new WebClient();

            tempfile = Path.GetTempFileName();

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
            Size.Content = "Downlaoding...";
            _webClient.DownloadFileAsync(new Uri(uri), tempfile);
        }

        void Log(string msg)
        {
            App.Log(msg);
        }


        //public async void HasUpdate()
        //{
        //    string? getEnv = null;
        //    Version? current = null;

        //    //try find installed version
        //    getEnv = Environment.GetEnvironmentVariable(nameof(Kebler), EnvironmentVariableTarget.User);

        //    if (!string.IsNullOrEmpty(getEnv))
        //    {
        //        Log($"We found old version on: {getEnv}");

        //        if (File.Exists(getEnv))
        //        {
        //            current = new Version(FileVersionInfo.GetVersionInfo(getEnv).FileVersion);
        //            Log($"Current version is: {current}");

        //            Log($"Okay. Try get server version (github version)");
        //            var result = await UpdaterApi.Check(ConstStrings.GITHUB_USER, nameof(Kebler), current, true);

        //            Log($"Server version is: {result.Item2.name}");

        //            if (result.Item2.name > current)
        //            {
        //                var updateUrl = result.Item2.assets.LastOrDefault().browser_download_url;
        //                Log($"So here is: {updateUrl}");

        //                await Application.Current.Dispatcher.InvokeAsync(() =>
        //                {
        //                    var wd = new MainWindow(new Uri(updateUrl));
        //                    wd.ShowDialog();
        //                    Current.Shutdown(0);
        //                });
        //            }
        //            else
        //            {
        //                Process.Start(getEnv);
        //                Current.Shutdown(0);
        //            }

        //        }
        //        else
        //        {
        //            startFree();

        //        }
        //    }
        //    else
        //    {
        //        startFree();
        //    }
        //}

      




























        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
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
            }
            catch
            {
                App.DONE(false);
                return;
            }
            App.DONE(true);
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
            Size.Content = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            PB.Value = e.ProgressPercentage;
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

        private void CustomizableWindow_Closing(object sender, CancelEventArgs e)
        {
            _webClient.CancelAsync();

            _webClient.DownloadProgressChanged -= OnDownloadProgressChanged;
            _webClient.DownloadFileCompleted -= WebClientOnDownloadFileCompleted;

            _webClient.Dispose();
            App.DONE(DialogResult);
        }
    }

    //public class MyWebClient : WebClient
    //{
    //    /// <summary>
    //    ///     Response Uri after any redirects.
    //    /// </summary>
    //    public Uri ResponseUri;

    //    /// <inheritdoc />
    //    protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
    //    {
    //        var webResponse = base.GetWebResponse(request, result);
    //        ResponseUri = webResponse.ResponseUri;
    //        return webResponse;
    //    }
    //}
}