using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Kebler.Dialogs;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Resources;
using Kebler.Services;
using Microsoft.Win32;
using Enums = Kebler.Models.Enums;
using MessageBox = Kebler.Dialogs.MessageBox;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow : INotifyPropertyChanged
    {
        public KeblerWindow()
        {


            InitializeComponent();
            ApplyConfig();

            DataContext = this;

            App.Instance.LangChanged += Instance_LangChanged;

            Task.Factory.StartNew(InitConnection);

            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_AllTorrents, Cat = Enums.Categories.All });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Downloading, Cat = Enums.Categories.Downloading });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Active, Cat = Enums.Categories.Active });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_InActive, Cat = Enums.Categories.Inactive });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Ended, Cat = Enums.Categories.Ended });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Stopped, Cat = Enums.Categories.Stopped });
            CategoriesListBox.Items.Add(new StatusCategory { Title = Strings.Cat_Error, Cat = Enums.Categories.Error });

            CategoriesListBox.SelectedIndex = 0;
            App.Instance.KeblerControl = this;
            Application.Current.MainWindow = this;
        }





        #region Config&HotKeys
        private void Init_HK()
        {

            lock (syncObjKeys)
            {
                if (RegisteredKeys == null)
                    RegisteredKeys = new[]
                    {
                        new HotKey(Key.C, KeyModifier.Shift | KeyModifier.Ctrl, ShowConnectionManager, this.GetWindowHandle()),
                        new HotKey(Key.N, KeyModifier.Ctrl, AddTorrent, this.GetWindowHandle())
                    };
            }
        }

        private void UnregisterHotKeys()
        {
            lock (syncObjKeys)
            {
                foreach (var key in RegisteredKeys)
                {
                    key.Dispose();
                }
                RegisteredKeys = null;
            }
        }

        private void ApplyConfig()
        {
            WorkingParams = new[]
            {
                TorrentFields.NAME,
                TorrentFields.ID,
                TorrentFields.ADDED_DATE,
                TorrentFields.HASH_STRING,
                TorrentFields.RATE_DOWNLOAD,
                TorrentFields.RATE_UPLOAD,
                TorrentFields.RECHECK,
                TorrentFields.PERCENT_DONE,
                TorrentFields.UPLOADED_EVER,
                TorrentFields.STATUS,
                TorrentFields.TRACKER_STATS,
                TorrentFields.DOWNLOAD_DIR,
                TorrentFields.FILES,

                //TODO: REMOVE 
                TorrentFields.PIECE_COUNT,
                TorrentFields.PIECES
            };
            try
            {

                MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

                CategoriesColumn.Width = new GridLength(ConfigService.Instanse.CategoriesWidth);
                MoreInfoColumn.Height = new GridLength(ConfigService.Instanse.MoreInfoHeight);
                Width = ConfigService.Instanse.MainWindowWidth;
                Height = ConfigService.Instanse.MainWindowHeight;
                WindowState = ConfigService.Instanse.MainWindowState;
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

                //if (!string.IsNullOrEmpty(ConfigService.Instanse.ColumnSizes))
                //{
                //    var splited = ConfigService.Instanse.ColumnSizes.Split(',');
                //    for (int i = 1; i < splited.Length; i++)
                //    {
                //        TorrentsDataGrid.Columns[i].Width = Convert.ToDouble(splited[i]);
                //    }
                //}


            }
            catch (Exception ex)
            {
                Log.Error(ex);

                var msg = new MessageBox(Kebler.Resources.Dialogs.ConfigApllyError, Kebler.Resources.Dialogs.Error, Enums.MessageBoxDilogButtons.Ok, true);
                msg.ShowDialog();
            }
        }

        private void SaveConfig()
        {
            var size = new StringBuilder();
            foreach (var column in TorrentsDataGrid.Columns)
            {
                size.Append($"{column.ActualWidth},");
            }


            ConfigService.Instanse.MainWindowHeight = ActualHeight;
            ConfigService.Instanse.MainWindowWidth = ActualWidth;
            ConfigService.Instanse.MainWindowState = WindowState;
            //ConfigService.Instanse.ColumnSizes = size.ToString().TrimEnd(',');

            ConfigService.Save();
        }
        #endregion





        #region ToolBar actions
        private async void SetLocation_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

           //var items = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToList();
            var question = selectedIDs.Length > 1 ? Kebler.Resources.Dialogs.SetLocForMany.Replace("%d", selectedIDs.Length.ToString())
                : Kebler.Resources.Dialogs.SetLocOnce;

            var path = ((TorrentInfo)TorrentsDataGrid.SelectedItem).DownloadDir;

            var dialog = new DialogBox(question, path, false) { Owner = Application.Current.MainWindow };

            if (dialog.ShowDialog() == true)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var itms = selectedIDs;
                    while (true)
                    {
                        if (Dispatcher.HasShutdownStarted) return;
                        var resp = await _transmissionClient.TorrentSetLocationAsync(itms, dialog.Value, true, _cancelTokenSource.Token);
                        resp.ParseTransmissionReponse(Log);

                        if (CheckResponse(resp))
                            break;

                        await Task.Delay(500, _cancelTokenSource.Token);
                    }
                }, _cancelTokenSource.Token);
            }

            dialog.Value = null;
        }

        public void ShowConnectionManager()
        {
            Dispatcher.Invoke(() =>
            {
                _cmWindow = new ConnectionManager(ref _dbServers, Application.Current.MainWindow);
                _cmWindow.ShowDialog();
                UpdateServers();
            });
        }

        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(IEnumerable<string> names)
        {
            foreach (var item in names)
            {
                var dialog = new AddTorrentDialog(item, _transmissionClient, this);

                if (!(bool)dialog.ShowDialog())
                    return;

                if (dialog.TorrentResult.Value.Status != Enums.AddTorrentStatus.Added) continue;

                TorrentList.Add(new TorrentInfo(dialog.TorrentResult.Value.ID)
                {
                    Name = dialog.TorrentResult.Value.Name,
                    HashString = dialog.TorrentResult.Value.HashString
                });
                dialog = null;
                OnPropertyChanged(nameof(TorrentList));
            }
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            IsErrorOccuredWhileConnecting = false;
            UpdateServers();
            InitConnection();
        }

        private async void TorrentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            SelectedTorrents = TorrentsDataGrid.SelectedItems.Cast<TorrentInfo>().ToArray();

            selectedIDs = SelectedTorrents.Select(x => x.Id).ToArray();
            await UpdateMoreInfoView();

        }



        public async Task UpdateMoreInfoView()
        {
            if (selectedIDs.Length != 1)
            {
                MoreInfo.IsMore = true;
                MoreInfo.Clear();
                MoreInfo.SelectedCount = selectedIDs.Length;
                return;
            }

            MoreInfo.IsMore = false;
            MoreInfo.Loading = true;

            var answ = await _transmissionClient.TorrentGetAsyncWithID(TorrentFields.ALL_FIELDS, _cancelTokenSource.Token, selectedIDs);

            var torrent = answ.Torrents.FirstOrDefault();
            MoreInfo.FilesTree.UpdateFilesTree(ref torrent);
            MoreInfo.PercentDone = torrent.PercentDone;
            answ = null;
            torrent = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();

            MoreInfo.Loading = false;
        }


        private async void FileTreeViewControl_OnFileStatusUpdate(uint[] wanted, uint[] unwanted, bool status)
        {
            var settings = new TorrentSettings
            {
                IDs = selectedIDs,
                FilesWanted = wanted.Any() ? wanted : null,
                FilesUnwanted = unwanted.Any() ? unwanted : null
            };

            if (wanted.Length > 0)
                Log.Info("wanted " + string.Join(", ", wanted));

            if (unwanted.Length > 0)
                Log.Info("unwanted " + string.Join(", ", unwanted));

            var resp =  await _transmissionClient.TorrentSetAsync(settings, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }




        private async void PauseTorrent_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private async void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStartAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);
        }

        private void AddTorrentButtonClick(object sender, RoutedEventArgs e)
        {
            AddTorrent();
        }

        private async void PauseAll_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            var resp = await _transmissionClient.TorrentStopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private async void StartTorrent_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;
            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentStartAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private void RemoveTorrent_Click(object sender, RoutedEventArgs e)
        {
            RemoveTorrent();
        }

        private void RemoveTorrentWithData_Click(object sender, RoutedEventArgs e)
        {
            RemoveTorrent(true);
        }

        private async void VerifyTorrent_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentVerifyAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private async void Reannounce_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.ReannounceTorrentsAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }


        private async void MoveTop_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveTopAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private async void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveUpAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        private async void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveDownAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private async void MoveBot_CLick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;
         
            //var torrents = SelectedTorrents.Select(x => x.Id).ToArray();
            var resp = await _transmissionClient.TorrentQueueMoveBottomAsync(selectedIDs, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        public void RemoveTorrent(bool removeData = false)
        {
            var toRemove = selectedIDs;
            var dialog = new RemoveTorrentDialog(SelectedTorrents.Select(x => x.Name).ToArray(), toRemove, ref _transmissionClient, removeData) { Owner = this };
            if ((bool)dialog.ShowDialog())
            {
                if (dialog.Result == Enums.RemoveResult.Ok)
                {
                    lock (_syncTorrentList)
                    {
                        foreach (var rm in toRemove)
                        {
                            var itm = TorrentList.First(x => x.Id == rm);
                            TorrentList.Remove(itm);

                            allTorrents.Torrents = allTorrents.Torrents.Where(val => val.Id == rm).ToArray();
                        }
                        if (allTorrents.Clone() is TransmissionTorrents data)
                        {
                            ProcessParsingTransmissionResponse(data);
                        }

                    }

                }
            }
        }

        private async void SlowMode_Click(object sender, RoutedEventArgs e)
        {
            IsSlowModeEnabled = !IsSlowModeEnabled;
            var resp = await _transmissionClient.SetSessionSettingsAsync(new SessionSettings { AlternativeSpeedEnabled = !_settings.AlternativeSpeedEnabled }, _cancelTokenSource.Token);
            resp.ParseTransmissionReponse(Log);

        }

        #endregion





        #region WindowEvents
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || !IsConnected)
                return;
            if (!(sender is ListBox listBox)) return;

            lock (_syncTorrentList)
            {
                _filterCategory = ((StatusCategory)listBox.SelectedValue).Cat;
            }

            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessParsingTransmissionResponse(data);
            }
        }


        private void KeblerWindow_OnActivated(object sender, EventArgs e)
        {
            Init_HK();
        }

        private void KeblerWindow_OnDeactivated(object sender, EventArgs e)
        {
            UnregisterHotKeys();
        }

        private void ClearFilter(object sender, MouseButtonEventArgs e)
        {
            FilterTextBox.Clear();
            FolderListBox.SelectedIndex = -1;
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            FilterText = FilterTextBox.Text;
            if (allTorrents.Clone() is TransmissionTorrents data)
            {
                ProcessParsingTransmissionResponse(data);
            }
        }

        private void ChangeFolderFilter(object sender, MouseButtonEventArgs e)
        {
            if (FolderListBox.SelectedItem is FolderCategory cat)
            {
                FilterTextBox.Text = $"{{p}}:{cat.FullPath}";
            }
        }

        private void Instance_LangChanged()
        {
            IsConnectedStatusText = $"Transmission {_sessionInfo?.Version} (RPC:{_sessionInfo?.RpcVersion})     " +
                                    $"      {Kebler.Resources.Windows.Stats_Uploaded} {Utils.GetSizeString(_stats.CumulativeStats.UploadedBytes)}" +
                                    $"      {Kebler.Resources.Windows.Stats_Downloaded}  {Utils.GetSizeString(_stats.CumulativeStats.DownloadedBytes)}" +
                                    $"      {Kebler.Resources.Windows.Stats_ActiveTime}  {TimeSpan.FromSeconds(_stats.CurrentStats.SecondsActive).ToPrettyFormat()}";
        }

        private static async Task<string> GetPassword()
        {
            var pass = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dialog = new MessageBox(true, "Enter password", true);
                dialog.Owner = Application.Current.MainWindow;
                try
                {
                    if (dialog.ShowDialog() == true)
                    {
                        pass = dialog.Value;
                    }
                }
                finally
                {

                    dialog.Value = null;
                    dialog = null;
                }
            });
            return pass;
        }

        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ConfigService.Instanse.CategoriesWidth = CategoriesColumn.ActualWidth;
            ConfigService.Instanse.MoreInfoHeight = MoreInfoColumn.ActualHeight;
            ConfigService.Save();

        }

        private void ClosingW(object sender, CancelEventArgs e)
        {
            SaveConfig();

            Log.Info("-----------Exit-----------");
        }
        #endregion

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



    }
}
