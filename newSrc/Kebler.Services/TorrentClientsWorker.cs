using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using Kebler.Workers;

namespace Kebler.Services
{
    public class TorrentClientsWorker : ITorrentClientsWorker
    {
        private static object _lock = new();
        private static readonly Dictionary<IServer, IRemoteTorrentClient> Clients = new();

        private static ITorrentClientsWorker _torrentClientsWorker;
        public static ITorrentClientsWorker Instance => _torrentClientsWorker ??= new TorrentClientsWorker();


        public async Task<(bool, Exception)> CheckConnectionAsync(IServer server)
        {
            GetTorrentClient(server, out var client);

            var connectionResult = await client.CheckConnectionAsync();
            lock (Clients)
            {
                if (Clients.ContainsKey(server))
                    Clients.Remove(server);
            }

            return connectionResult;
        }

        private static void GetTorrentClient(in IServer server, out IRemoteTorrentClient remoteClient)
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
                        remoteClient = TransmissionWorker.CreateInstance(server, IoC.Get<ILogger>());
                        Clients.Add(server, remoteClient);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
