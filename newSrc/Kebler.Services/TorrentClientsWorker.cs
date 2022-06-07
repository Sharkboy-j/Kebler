using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<IServer, IWorker> Clients = new();

        //private static ITorrentClientsWorker _torrentClientsWorker;
        //public static ITorrentClientsWorker Instance => _torrentClientsWorker ??= new TorrentClientsWorker();

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

            lock (Clients)
            {
                if (Clients.TryGetValue(server, out remoteClient))
                {
                    return;
                }

                switch (server.ClientType)
                {
                    case ClientType.Transmission:
                        remoteClient = TransmissionWorker.CreateInstance(in server, IoC.Get<ILogger>(), in password);
                        break;
                    case ClientType.QBittorrent:
                        remoteClient = QBitTorrentWorker.CreateInstance(in server, IoC.Get<ILogger>(), in password);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Clients.Add(server, remoteClient);
            }
        }

        public async Task<(bool, Exception)> CheckConnectionAsync(IServer server, bool disconnectAfter = false)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var connectionResult = await client.CheckConnectionAsync(disconnectAfter: disconnectAfter);
            
            //lock (Clients)
            //{
            //    if (Clients.ContainsKey(server))
            //        Clients.Remove(server);
            //}

            return connectionResult;
        }

        public async Task<IStatistic> GetSessionStatisticAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = await client.GetSessionStatisticAsync(token);

            return response.Item1;
        }

        public async Task<IEnumerable<ITorrent>> TorrentsGetAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = await client.TorrentsGetAsync(token);

            return response.Item1;
        }

        public async Task<ITorrentClientSettings> GetTorrentClientSettingsAsync(IServer server, CancellationToken token)
        {
            GetOrCreateTorrentClient(in server, null, out var client);

            var response = await client.GetTorrentClientSettingsAsync(token);

            return response.Item1;
        }
    }
}
