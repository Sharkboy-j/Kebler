using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;
using Newtonsoft.Json;

namespace Kebler.UI.Models
{
    public class TorrentFileStats : PropertyChangedBase, ITorrentFileStats
    {
        private double _bytesCompleted;
        private bool _wanted;
        private int _priority;

        public double BytesCompleted
        {
            get => _bytesCompleted;
            set => Set(ref _bytesCompleted, value);
        }

        public bool Wanted
        {
            get => _wanted;
            set => Set(ref _wanted, value);
        }

        public int Priority
        {
            get => _priority;
            set => Set(ref _priority, value);
        }
    }
}
