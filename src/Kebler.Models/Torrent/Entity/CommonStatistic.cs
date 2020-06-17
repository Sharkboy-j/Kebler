using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Entity
{
    public class CommonStatistic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonProperty("uploadedBytes")]
        public long UploadedBytes{ get; set; }
        
        [JsonProperty("downloadedBytes")]
        public long DownloadedBytes{ get; set; }

        [JsonProperty("filesAdded")]
        public int FilesAdded{ get; set; }

        [JsonProperty("SessionCount")]
        public int SessionCount{ get; set; }

        [JsonProperty("SecondsActive")]
        public int SecondsActive{ get; set; }
    }
}