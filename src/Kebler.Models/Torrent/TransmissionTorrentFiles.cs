using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentFiles : INotifyPropertyChanged
    {
        [JsonConstructor]
        public TransmissionTorrentFiles()
        {
        }

        public TransmissionTorrentFiles(long pLen, params string[] fullPath)
        {
            Length = pLen;
            Name = Path.Combine(fullPath);
        }

        [JsonProperty("bytesCompleted")]
        public double BytesCompleted { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}