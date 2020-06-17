using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Entity
{
    public class Statistic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonProperty("activeTorrentCount")]
        public int ActiveTorrentCount { get; set; }

        [JsonProperty("downloadSpeed")]
        public int DownloadSpeed{ get; set; }

        [JsonProperty("pausedTorrentCount")]
        public int PausedTorrentCount{ get; set; }

        [JsonProperty("torrentCount")]
        public int TorrentCount{ get; set; }

        [JsonProperty("uploadSpeed")]
        public int UploadSpeed{ get; set; }
   
        [JsonProperty("cumulative-stats")]
        public CommonStatistic CumulativeStats { get; set; }
 
        [JsonProperty("current-stats")]
        public CommonStatistic CurrentStats { get; set; }
    }
}
