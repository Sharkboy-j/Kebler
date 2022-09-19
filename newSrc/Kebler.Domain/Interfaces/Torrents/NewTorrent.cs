using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Domain.Interfaces.Torrents
{
    public  class NewTorrent : INewTorrent, INotifyPropertyChanged
    {
        public string DownloadDir { get; set; }
        public string HashString { get; set; }
        public string FilePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
