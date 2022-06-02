using System;
using System.Threading.Tasks;

namespace Kebler.Domain.Interfaces
{
    public interface ITorrentClientsWorker
    {
        public Task<(bool, Exception)> CheckConnectionAsync(IServer server);
    }
}