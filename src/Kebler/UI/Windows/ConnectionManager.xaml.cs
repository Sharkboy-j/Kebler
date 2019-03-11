using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models;
using Kebler.Services;
using LiteDB;

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for ConnectionManager.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class ConnectionManager : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelectedServerTabEnabled { get; set; } = true;
        public Server SelectedServer { get; set; }// = new Server();
        public ObservableCollection<Server> ServerList = new ObservableCollection<Server>();
        private LiteCollection<Server> DbServersList;

        public ConnectionManager()
        {
            InitializeComponent();
            this.DataContext = this;

            GetServers();
        }


        //TODO: Add background loading
        private void GetServers()
        {
            DbServersList = StorageRepository.ServersList();
            var items = DbServersList?.FindAll();
            items = items ?? new List<Server>();

            ServerList = new ObservableCollection<Server>(items);
            ServersListBox.ItemsSource = ServerList;

        }


        //TODO: add background 
        private void AddNewServer_ButtonClick(object sender, RoutedEventArgs e)
        {
            var id = ServerList.Count == 0 ? 1 : ServerList[ServerList.Count - 1].Id + 1;

            var server = new Server() { Id =id, Title = $"Transmission Server {ServerList.Count+1}",AskForPassword = false,AuthEnabled = false};
            DbServersList.Insert(server);
            //var result = DbServersList.(x => x.Title);
            GetServers();
        }

        private void ServersListBox_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServersListBox.SelectedItems.Count > 0)
            {
                SelectedServer = ServersListBox.SelectedItems[0] as Server;
            }
         
        }

        //TODO: Add to collection and storage
        private void SaveServer_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (ValidateServer(SelectedServer, out var error))
            {
                DbServersList.Upsert(SelectedServer);
                GetServers();
            }
            else
            {
                MessageBox.Show(error.ToString());
            }
          
        }

        private bool ValidateServer(Server server, out Enums.ValidateError error)
        {
            error = Enums.ValidateError.Ok;
            if (string.IsNullOrEmpty(server.Title))
            {
                error = Enums.ValidateError.TitileEmpty;
                return false;
            }

            if (string.IsNullOrEmpty(server.Host))
            {
                error = Enums.ValidateError.IpOrHostError;
                return false;
            }
            //TODO: Chech ipadress port
            return true;
        }
        private void CloseServer_ButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedServer = null;

        }


        //TODO: add background removing
        private void RemoveServer_ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedServer == null) return;

           var result =  StorageRepository.ServersList().Delete((int)SelectedServer.Id);
           if (!result)
           {
               //TODO: Add string 
               MessageBox.Show("RemoveErrorContent");
           }

           SelectedServer = null;
           GetServers();
        }
    }
}
