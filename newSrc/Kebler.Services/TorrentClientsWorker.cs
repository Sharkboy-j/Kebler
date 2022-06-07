using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Workers;

namespace Kebler.Services
{
    public class TorrentClientsWorker : ITorrentClientsWorker
    {
        private static object _lock = new();
        private static readonly Dictionary<IServer, IWorker> Clients = new();

        private static ITorrentClientsWorker _torrentClientsWorker;
        public static ITorrentClientsWorker Instance => _torrentClientsWorker ??= new TorrentClientsWorker();

        public void Init(in IServer server, in string password)
        {
            GetOrCreateTorrentClient(in server, in password, out _);
        }

        public void Close(in IServer server)
        {
            lock (Clients)
            {
                Clients.Remove(server);
            }
        }

        private static void GetOrCreateTorrentClient(in IServer server, in string password, out IWorker remoteClient)
        {
            var pass = password ?? SecureStorage.DecryptStringAndUnSecure(server.Password);

            lock (Clients)
            {
                if (Clients.TryGetValue(server, out remoteClient))
                {
                    return;
                }

                switch (server.ClientType)
                {
                    case ClientType.Transmission:
                        remoteClient = TransmissionWorker.CreateInstance(in server, IoC.Get<ILogger>(), in pass);
                        Clients.Add(server, remoteClient);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public async Task<(bool, Exception)> CheckConnectionAsync(IServer server)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var connectionResult = await client.CheckConnectionAsync();

            lock (Clients)
            {
                if (Clients.ContainsKey(server))
                    Clients.Remove(server);
            }

            return connectionResult;
        }

        public Task<IStatistic> GetSessionStatisticAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = client.GetSessionStatisticAsync(token);

            return response;
        }

        public Task<IEnumerable<ITorrent>> TorrentsGetAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = client.TorrentsGetAsync(token);

            return response;
        }

        public Task<ITorrentClientSettings> GetTorrentClientSettingsAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = client.GetTorrentClientSettingsAsync(token);

            return response;
        }
    }
}
