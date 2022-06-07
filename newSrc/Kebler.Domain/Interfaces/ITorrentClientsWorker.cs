using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.Domain.Interfaces
{
    public interface ITorrentClientsWorker
    {
        public Task<(bool, Exception)> CheckConnectionAsync(IServer server);

        public void Init(in IServer server, in string password);

        public void Close(in IServer server);

        public Task<IStatistic> GetSessionStatisticAsync(IServer server, CancellationToken token);

        public Task<IEnumerable<ITorrent>> TorrentsGetAsync(IServer server, CancellationToken token);

        public Task<ITorrentClientSettings> GetTorrentClientSettingsAsync(IServer server, CancellationToken token);
    }
}