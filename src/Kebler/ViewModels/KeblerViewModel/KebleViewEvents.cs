using Kebler.Models.Torrent;
using System.Linq;
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
        public void Properties(object sender, TorrentInfo torrentRow)
        {
            if (_transmissionClient != null)
            {
                if (selectedIDs.ToArray() is uint[] ids && ids.Length > 0)
                {
                    manager.ShowDialogAsync(new TorrentPropsViewModel(
                        _transmissionClient,
                        ids,
                        manager));
                }
                else if (sender is ListView && torrentRow is TorrentInfo selectedTorrent)
                {
                    manager.ShowDialogAsync(new TorrentPropsViewModel(_transmissionClient, new[] { torrentRow.Id }, manager));
                }
            }

        }
    }
}
