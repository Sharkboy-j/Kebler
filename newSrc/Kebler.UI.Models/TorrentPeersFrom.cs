using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
    public class TorrentPeersFrom : PropertyChangedBase, ITorrentPeersFrom
    {
        private int _fromDht;
        private int _fromIncoming;
        private int _fromLpd;
        private int _fromLtep;
        private int _fromPex;
        private int _fromTracker;

        public int FromDHT
        {
            get => _fromDht;
            set => Set(ref _fromDht, value);
        }

        public int FromIncoming
        {
            get => _fromIncoming;
            set => Set(ref _fromIncoming, value);
        }

        public int FromLPD
        {
            get => _fromLpd;
            set => Set(ref _fromLpd, value);
        }

        public int FromLTEP
        {
            get => _fromLtep;
            set => Set(ref _fromLtep, value);
        }

        public int FromPEX
        {
            get => _fromPex;
            set => Set(ref _fromPex, value);
        }

        public int FromTracker
        {
            get => _fromTracker;
            set => Set(ref _fromTracker, value);
        }
    }
}
