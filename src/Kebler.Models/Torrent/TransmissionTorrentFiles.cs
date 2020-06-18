using System;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentFiles : INotifyPropertyChanged
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonConstructor]
        public TransmissionTorrentFiles()
        {

        }

        public TransmissionTorrentFiles(long pLen, params string[] fullPath)
        {
            Length = pLen;
            Name = System.IO.Path.Combine(fullPath);
        }
    }
}
