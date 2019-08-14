using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Kebler.Models;
using Kebler.Services;
using log4net;
using LiteDB;
using Transmission.API.RPC;
using System.Threading.Tasks;
using Kebler.UI.Windows.Dialogs;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Transmission.API.RPC.Entity;
using System.Threading;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Kebler.UI.ViewModels;
using Kebler.Services.Converters;
using Microsoft.Win32;
using System.Windows.Interop;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="Kebler" />
    /// <summary>
    /// Interaction logic for Kebler.xaml
    /// </summary>
    public partial class Kebler : Window
    {

        private MainWindowViewModel Vm => this.DataContext as MainWindowViewModel;

     
        public Kebler()
        {
            //HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            //source.AddHook(new HwndSourceHook(WndProc));

          
            InitializeComponent();

            //disable hardware rendering
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            DataContext = new MainWindowViewModel();

        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_COPYDATA:
                    MessageBox.Show("");
                    Win32.CopyDataStruct st = (Win32.CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(Win32.CopyDataStruct));
                    string strData = Marshal.PtrToStringUni(st.lpData);

                    foreach (var text in strData.Split(' ')) 
                    {
                        if (text.Contains(".torrent"))
                        {
                            OpenTorrent(new[] {text});
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
            var dialog = new DialogBox("You are perform to remove %n torrents with data","Please, confirm action.");
            dialog.ShowDialog();
            if(dialog.Response)
            {
                Vm.RemoveTorrent(true);
            }
        }
    }
}
