using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.Domain.Interfaces
{
    public interface IWorker
    {
        public Task<(bool, Exception)> CheckConnectionAsync(int timeOut = default);

        public Task<IStatistic> GetSessionStatisticAsync(CancellationToken token);

        public Task<IEnumerable<ITorrent>> TorrentsGetAsync(CancellationToken token);

        public Task<ITorrentClientSettings> GetTorrentClientSettingsAsync(CancellationToken token);

    }
}
