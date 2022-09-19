using Kebler.Domain.Interfaces.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Domain.Models
{
    public class AddTorrentResult
    {
        public AddTorrentResult(bool isSuccess, Exception exception, INewTorrent torrent)
        {
            IsSuccess = isSuccess;
            Exception = exception;
            Torrent = torrent;
        }

        public bool IsSuccess { get; private set; }

        public Exception Exception { get; private set; }

        public INewTorrent Torrent { get; private set; }
    }
}
