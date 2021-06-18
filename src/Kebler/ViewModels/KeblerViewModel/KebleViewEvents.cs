using Kebler.Models.Torrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Kebler.ViewModels
{
    /// <summary>
    /// ViewModel for events triggering.
    /// </summary>
    public partial class KeblerViewModel
    {
     
        /// <summary>
        /// Open torrent properties popUp by double click on grid or RightClickMenu on torrent row.
        /// </summary>
        /// <param name="sender">sender...</param>
        /// <param name="torrentRow">Torrent..</param>
        public void Properties(object? sender, TorrentInfo? torrentRow)
        {
            if (_transmissionClient != null)
            {
                if (selectedIDs != null)
                {
                    if (selectedIDs.Length > 1)
                    {
                        manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient,
                            SelectedTorrents.Select(x => x.Id).ToArray(), manager));
                    }
                    else
                    {
                        manager.ShowDialogAsync(new TorrentPropsViewModel(
                            _transmissionClient,
                            new[] { SelectedTorrents.Select(x => x.Id).First() },
                            manager));
                    }
                }
                else if (sender is ListView && torrentRow is TorrentInfo selectedTorrent)
                {
                    manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient, new[] { torrentRow.Id }, manager));
                }
            }

        }
    }
}
