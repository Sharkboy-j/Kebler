using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kebler.Models;
using Kebler.Models.Interfaces;
using Kebler.Services;
using Kebler.TransmissionCore;
using Kebler.UI.Annotations;
using LiteDB;
using log4net;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Kebler.Views
{
    /// <summary>
    /// Interaction logic for ConnectionManagerView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class ConnectionManagerView : IConnectionManager
    {


        public PasswordBox pwd { get; }
    
        public ConnectionManagerView()
        {
            InitializeComponent();
            pwd = this.ServerPasswordBox;
        }


        //public override void Onload(object sender, RoutedEventArgs args)
        //{
        //    base.Onload(sender, args);

        //    GetServers();
        //}


        ////TODO: Add background loading
        //private void GetServers(int selectedId = -1)
        //{
        //    _dbServersList = StorageRepository.GetServersList();

        //    var items = _dbServersList?.FindAll() ?? new List<Server>();

        //    ServerList.Clear();
        //    ServerList.AddRange(items);

        //    if (selectedId != -1)
        //    {
        //        ServerIndex = selectedId;

        //        foreach (var item in ServersListBox.Items)
        //        {
        //            if (!(item is Server server)) continue;
        //            if (server.Id != selectedId) continue;

        //            var ind = ServersListBox.Items.IndexOf(item);
        //            ServersListBox.SelectedIndex = ind;
        //            break;
        //        }
        //    }
        //    else
        //    {
        //        ServerIndex = 0;
        //    }


        //}



        //private void AddNewServer_ButtonClick(object sender, RoutedEventArgs e)
        //{

        //    var server = new Server { Title = $"Transmission Server {ServerList.Count + 1}", AskForPassword = false, AuthEnabled = false };
        //    _dbServersList.Insert(server);

        //    Log.Info($"Add new Server {server}");

        //    ServerList.Add(server);
        //    OnPropertyChanged(nameof(ServerList));
        //    // App.InvokeServerListChanged();

        //}

        //private void ServersListBox_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ServersListBox.SelectedItems.Count != 1) return;

        //    SelectedServer = ServersListBox.SelectedItems[0] as Server;
        //    ServerPasswordBox.Password =
        //        SecureStorage.DecryptStringAndUnSecure(SelectedServer?.Password);
        //    ConnectStatusResult = string.Empty;

        //}

        ////TODO: Add to collection and storage
       

        //private static bool ValidateServer(Server server, out Enums.ValidateError error)
        //{
        //    error = Enums.ValidateError.Ok;
        //    if (string.IsNullOrEmpty(server.Title))
        //    {
        //        error = Enums.ValidateError.TitileEmpty;
        //        return false;
        //    }

        //    if (!string.IsNullOrEmpty(server.Host)) return true;

        //    error = Enums.ValidateError.IpOrHostError;
        //    return false;
        //    //TODO: Check ipadress port
        //}
        //private void CloseServer_ButtonClick(object sender, RoutedEventArgs e)
        //{
        //    SelectedServer = null;
        //    ServersListBox.SelectedIndex = -1;
        //}


        ////TODO: add background removing
        //private void RemoveServer_ButtonClicked(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedServer == null) return;


        //    Log.Info($"Try remove server: {SelectedServer}");

        //    var result = StorageRepository.GetServersList().Delete(SelectedServer.Id);
        //    Log.Info($"RemoveResult: {result}");
        //    if (!result)
        //    {
        //        //TODO: Add string 
        //        System.Windows.MessageBox.Show("RemoveErrorContent");
        //    }
        //    var ind = ServersListBox.SelectedIndex -= 1;

        //    //if (App.Instance.KeblerControl.SelectedServer.Id == SelectedServer.Id)
        //    //{
        //    //    App.Instance.KeblerControl.Disconnect();
        //    //}

        //    SelectedServer = null;
        //    GetServers();
        //    App.InvokeServerListChanged();
        //    ServersListBox.SelectedIndex = ind;
        //}

        //private async void TestConnection_ButtonClick(object sender, RoutedEventArgs e)
        //{

        //    string pass = null;
        //    if (SelectedServer.AskForPassword)
        //    {
        //        var dialog = new MessageBox(true, "Enter password", true, string.Empty);
        //        dialog.Owner = this;
        //        try
        //        {
        //            if (dialog.ShowDialog() == true)
        //            {
        //                pass = dialog.Value;
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        }
        //        finally
        //        {

        //            dialog.Value = null;
        //            dialog = null;
        //        }
        //    }


        //    IsTesting = true;
        //    var result = await TesConnection(pass);
        //    ConnectStatusResult = result
        //        ? Kebler.Resources.Windows.CM_TestConnectionGood
        //        : Kebler.Resources.Windows.CM_TestConnectionBad;

        //    ConnectStatusColor = (result
        //        ? new SolidColorBrush { Color = Colors.Green }
        //        : new SolidColorBrush { Color = Colors.Red });

        //    IsTesting = false;
        //}

        //private async Task<bool> TesConnection(string pass)
        //{
        //    Log.Info("Start TestConnection");
        //    var scheme = SelectedServer.SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

        //    var uri = new UriBuilder(scheme, SelectedServer.Host, SelectedServer.Port, SelectedServer.RpcPath);

        //    try
        //    {
        //        var user = (bool)AuthEnabled.IsChecked ? SelectedServer.UserName : null;
        //        var pswd = (bool)AuthEnabled.IsChecked ? pass ?? ServerPasswordBox.Password : null;

        //        var _client = new TransmissionClient(uri.Uri.AbsoluteUri, null, user, pswd);

        //        var sessionInfo = await _client.GetSessionInformationAsync(new System.Threading.CancellationToken());
        //        if (sessionInfo == null)
        //            throw new Exception("Error while testing");

        //        await _client.CloseSessionAsync(new System.Threading.CancellationToken());
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message, ex);
        //        return false;
        //    }
        //    finally
        //    {
        //        _client = null;
        //    }


        //    return true;
        //}

        //private void SSL_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedServer != null)
        //        SelectedServer.SslEnabled = true;
        //}

        //private void SSL_Uncheked(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedServer != null)
        //        SelectedServer.SslEnabled = false;
        //}

        //private void CustomWindow_Closing(object sender, CancelEventArgs e)
        //{
        //    if (_client != null)
        //    {
        //        _client = null;
        //    }

        //    //if (ServerList.Count != 0)
        //    //{
        //    //    App.Instance.KeblerControl.InitConnection();
        //    //}

        //}

        //[NotifyPropertyChangedInvocator]
        //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
    }
}
