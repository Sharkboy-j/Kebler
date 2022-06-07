using System.IO;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
    public class TorrentFiles : PropertyChangedBase, ITorrentFiles
    {
        private long _bytesCompleted;
        private long _length;
        private string _name;

        public TorrentFiles()
        {
        }

        public TorrentFiles(long pLen, params string[] fullPath)
        {
            Length = pLen;
            Name = Path.Combine(fullPath);
        }

        public long BytesCompleted
        {
            get => _bytesCompleted;
            set => Set(ref _bytesCompleted, value);
        }

        public long Length
        {
            get => _length;
            set => Set(ref _length, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string[] NameParts => Name.Split('/');
    }
}
