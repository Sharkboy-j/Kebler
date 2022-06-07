using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces;

namespace Kebler.Domain.Models.Events
{
    public class ReconnectRequested
    {
        public ReconnectRequested(IServer server)
        {
            Server = server;
        }

        public IServer Server;
    }
}
