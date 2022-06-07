using Kebler.Domain.Interfaces;

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
