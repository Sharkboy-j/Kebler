using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using Kebler.UI.Windows.Dialogs;
using System.Windows.Media;
using Transmission.API.RPC.Entity;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Kebler.UI.ViewModels;
using System.Windows.Interop;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="KeblerWindow" />
    /// <summary>
    /// Interaction logic for Kebler.xaml
    /// </summary>
    public partial class KeblerWindow : Window
    {

        private MainWindowViewModel Vm => this.DataContext as MainWindowViewModel;
        NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();


        public KeblerWindow()
        {
            //HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //source.AddHook(new HwndSourceHook(WndProc));



            InitializeComponent();

            //disable hardware rendering
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            DataContext = new MainWindowViewModel();


            //var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "Kebler.Theme.Icons.Kebler.ico";

            //using var stream = assembly.GetManifestResourceStream(resourceName);
            //nIcon.Icon = new Icon(stream);
            //nIcon.Visible = true;
            //nIcon.ShowBalloonTip(5000, "Title", "Text", System.Windows.Forms.ToolTipIcon.Info);
            //nIcon.Click += NIcon_Click;

        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        public void UpdateSorting()
        {
            Vm.UpdateSortingManualy();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_COPYDATA:
                    Win32.CopyDataStruct st = (Win32.CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(Win32.CopyDataStruct));
                    string strData = Marshal.PtrToStringUni(st.lpData);

                    foreach (var text in strData.Split(' '))
                    {
                        if (text.Contains(".torrent"))
                        {
                            OpenTorrent(new[] { text });
                        }
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }


        public void OpenConnectionManager()
        {
            Vm.ShowConnectionManager();
        }
        public void Connect()
        {
            Vm.InitConnection();
        }
        public void AddTorrent()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true) return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(string[] names)
        {
            foreach (var item in names)
            {
                Vm.AddTorrent(item);
            }
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            // new Task(() => { VM.TryConnect(ServersList.FirstOrDefault()); }).Start();

        }

        private void TorrentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TorrentsDataGrid.SelectedValue is TorrentInfo tor)
            {
                Vm.SelectedTorrent = tor;
            }
        }

        private void RemoveTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent();
        }
        private void RemoveTorrentData_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent(true);
        }

        private void PauseTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.PauseTorrent();
        }

        private void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            Vm.StartAll();
        }

        private void AddTorrentButtonClick(object sender, RoutedEventArgs e)
        {
            AddTorrent();
        }

        private void PauseAll_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.StopAll();
        }

        private void StartSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.StartOne();
        }

        private void PauseSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.PauseTorrent();
        }

        private void RemoveTorrent_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent();
        }
        private void RemoveTorrentWithData_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent(true);

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        private void NIcon_Click(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
            Show();
            Activate();
        }

        private void Window_StateChanged_1(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }
    }
}
