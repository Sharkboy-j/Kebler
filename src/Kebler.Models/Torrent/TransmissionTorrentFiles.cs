using Newtonsoft.Json;
using System.ComponentModel;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentFiles : INotifyPropertyChanged
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        //~TransmissionTorrentFiles()
        //{
        //    Name = null;
        //    PropertyChanged = null;
        //}
    }
}
