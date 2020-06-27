using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Uri uri;
        private MyWebClient _webClient;
        private DateTime _startedAt;
        private string tempfile;
        public MainWindow(Uri uri)
        {
            InitializeComponent();
            this.uri = uri;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _webClient = new MyWebClient();

            tempfile = System.IO.Path.GetTempFileName();

            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;

            _webClient.DownloadFileTaskAsync(uri, tempfile);
        }

        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            ContentDisposition contentDisposition = null;
            if (_webClient.ResponseHeaders?["Content-Disposition"] != null)
            {
                contentDisposition = new ContentDisposition(_webClient.ResponseHeaders["Content-Disposition"]);
            }

            var fileName = contentDisposition.FileName;

            var pth = Path.Combine(Path.GetTempPath(), fileName);
            if (File.Exists(pth))
            {
                File.Delete(pth);
            }
            File.Move(tempfile, pth);

            string extractionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler));
            string keblerexe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler), nameof(App), $"{nameof(Kebler)}.exe");

            foreach (var process in Process.GetProcessesByName("Kebler.exe"))
            {
                process.Kill();
            }

            if (Directory.Exists(extractionPath))
                Directory.Delete(extractionPath, true);
            var zip = new ZipArchive(new FileStream(pth, FileMode.Open));
            zip.ExtractToDirectory(extractionPath,true);
            
            var processStartInfo = new ProcessStartInfo
            {
                FileName = keblerexe,
            };
            Process.Start(processStartInfo);
            Close();
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //if (_startedAt == default)
            //{
            //    _startedAt = DateTime.Now;
            //}
            //else
            //{
            //    var timeSpan = DateTime.Now - _startedAt;
            //    long totalSeconds = (long)timeSpan.TotalSeconds;
            //    if (totalSeconds > 0)
            //    {
            //        var bytesPerSecond = e.BytesReceived / totalSeconds;
            //        //Speed.Content = string.Format(BytesToString(bytesPerSecond)+"/s");
            //    }
            //}

            Size.Content = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            PB.Value = e.ProgressPercentage;
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

    }

    public class MyWebClient : WebClient
    {
        /// <summary>
        ///     Response Uri after any redirects.
        /// </summary>
        public Uri ResponseUri;

        /// <inheritdoc />
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse webResponse = base.GetWebResponse(request, result);
            ResponseUri = webResponse.ResponseUri;
            return webResponse;
        }
    }
}
