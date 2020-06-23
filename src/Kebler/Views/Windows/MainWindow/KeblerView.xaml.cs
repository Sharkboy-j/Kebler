using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using log4net;
using Microsoft.Win32;
using Enums = Kebler.Models.Enums;
using MessageBox = Kebler.Dialogs.MessageBox;

namespace Kebler.Views
{
    public partial class KeblerView
    {

        public KeblerView()
        {
            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;



            //App.Instance.LangChanged += Instance_LangChanged;

            //Task.Factory.StartNew(InitConnection);

            //App.Instance.KeblerControl = this;
            //Application.Current.MainWindow = this;
        }







        #region ToolBar actions
      

        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Torrent files (*.torrent)|*.torrent|All files (*.*)|*.*",
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
            }
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





     

        #endregion





        #region WindowEvents



     

     

        private void ClearFilter(object sender, MouseButtonEventArgs e)
        {
            FilterTextBox.Clear();
            FolderListBox.SelectedIndex = -1;
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!IsLoaded)
            //    return;
            //FilterText = FilterTextBox.Text;
            //if (allTorrents.Clone() is TransmissionTorrents data)
            //{
            //    ProcessParsingTransmissionResponse(data);
            //}
        }

        private void ChangeFolderFilter(object sender, MouseButtonEventArgs e)
        {
            if (FolderListBox.SelectedItem is FolderCategory cat)
            {
                FilterTextBox.Text = $"{{p}}:{cat.FullPath}";
            }
        }



        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ConfigService.Instanse.CategoriesWidth = CategoriesColumn.ActualWidth;
            ConfigService.Instanse.MoreInfoHeight = MoreInfoColumn.ActualHeight;
            ConfigService.Save();
        }

       
        #endregion


    }
}
