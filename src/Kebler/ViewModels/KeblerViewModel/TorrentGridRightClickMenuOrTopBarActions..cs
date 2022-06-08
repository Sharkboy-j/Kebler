using Kebler.Services;
using System.Linq;
using System.Threading;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once CheckNamespace

namespace Kebler.ViewModels
{
    /// <summary>
    /// Torrent Grid right click menu or top bar actions.
    /// </summary>
    public partial class KeblerViewModel
    { 
  

        /// <summary>
        /// Remove torrent without data.
        /// </summary>
        /// <param name="id">Torrent id.</param>
        private async void RemoveTorrent(uint id)
        {
            if (_transmissionClient is not null)
            {
                var toRemoveIds = new[] { id };

                var result = await _transmissionClient.TorrentRemoveAsync(toRemoveIds, new CancellationToken(), false);


                foreach (var toRemoveId in toRemoveIds)
                {
                    var itm = TorrentList.First(x => x.Id == toRemoveId);
                    TorrentList.Remove(itm);

                    //FOR WHAT IS THAT?! 
                    //TODO: Remove maybe
                    allTorrents.Torrents = allTorrents.Torrents.Where(torrentInfo => torrentInfo.Id == toRemoveId).ToArray();
                }
            }
        }

        public void Properties()
        {
            Log.Ui();
            Properties(null, null);
        }
    }
}