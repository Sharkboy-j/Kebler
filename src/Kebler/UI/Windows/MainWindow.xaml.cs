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
using Kebler.UI.ViewModels;
using Kebler.Services.Converters;
using Microsoft.Win32;
using System.Windows.Interop;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="MainWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainWindowViewModel Vm => this.DataContext as MainWindowViewModel;

        

        public MainWindow()
        {
            InitializeComponent();

            //disable hardware rendering
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            DataContext = new MainWindowViewModel();

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

            foreach(var item in openFileDialog.FileNames)
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
    }
}
