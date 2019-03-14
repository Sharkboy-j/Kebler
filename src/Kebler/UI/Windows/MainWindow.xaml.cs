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

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="MainWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly MainWindowViewModel VM;

        

        public MainWindow()
        {
            //log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            VM = new MainWindowViewModel();
            DataContext = VM;

        }

        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
           // new Task(() => { VM.TryConnect(ServersList.FirstOrDefault()); }).Start();

        }

        private void TorrentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if(TorrentsDataGrid.SelectedValue is TorrentInfo tor)
            {
                VM.SelectedTorrent = tor;
            }
        }
    }
}
