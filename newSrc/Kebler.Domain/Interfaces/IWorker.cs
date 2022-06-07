using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.Domain.Interfaces
{
    public interface IWorker
    {
        public Task<(bool, Exception)> CheckConnectionAsync(int timeOut = 10000, bool disconnectAfter = false);

        public Task<(bool, Exception)> CheckConnectionAsync(TimeSpan timeOut, bool disconnectAfter = false);

        public Task<(IStatistic, Exception)> GetSessionStatisticAsync(CancellationToken token);

        public Task<(IEnumerable<ITorrent>, Exception)> TorrentsGetAsync(CancellationToken token);

        public Task<(ITorrentClientSettings, Exception)> GetTorrentClientSettingsAsync(CancellationToken token);

    }
}
