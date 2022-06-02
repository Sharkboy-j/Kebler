using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Domain.Interfaces
{
    public interface IRemoteTorrentClient
    {
        public Task<(bool, Exception)> CheckConnectionAsync(int timeOut = default);
    }
}
