using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.Domain.Models.Events
{
    public class ConnectedServerChanged
    {
        public ConnectedServerChanged(IServer server)
        {
            Server = server;
        }

        public readonly IServer Server;
    }
}
