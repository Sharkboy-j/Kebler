using System.ComponentModel;

namespace Kebler.Domain.Interfaces.Torrents
{
    public interface INewTorrent : INotifyPropertyChanged
    {
        string DownloadDir { get; set; }
      
        string HashString { get; set; }
      
        string FilePath { get; set; }
    }
}
