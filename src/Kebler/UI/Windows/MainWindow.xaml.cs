using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kebler.Models;
using Kebler.Services;
using log4net;
using LiteDB;
using Transmission.API.RPC;
using Transmission.API.RPC.Entity;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="MainWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ConnectionManager CMWindow;
        private LiteCollection<Server> DBServers;
        private List<Server> ServersList;
        private Server SelectedServerTest;

        public MainWindow()
        {
            //log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();
            DBServers = StorageRepository.ServersList();
            ShowCMWindow();
        }

        private void GetServers()
        {
            DBServers = StorageRepository.ServersList();
            ServersList = new List<Server>(DBServers.FindAll());

            if (ServersList.Count == 0)
            {
                ShowCMWindow();
            }
            else
            {
                SelectedServerTest = ServersList.FirstOrDefault();
                //TODO: Init connection to all servers
            }
        }

        private void ShowCMWindow()
        {
            CMWindow = new ConnectionManager(DBServers);
            CMWindow.ShowDialog();
        }

      
    }
}
