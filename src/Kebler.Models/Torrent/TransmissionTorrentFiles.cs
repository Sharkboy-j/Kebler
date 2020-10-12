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
        public long BytesCompleted { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public string[] NameParts
        {
            get
            {
                return Name.Split('/');
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}