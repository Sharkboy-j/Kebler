using System.ComponentModel;
using Newtonsoft.Json;

namespace Transmission.API.RPC.Entity
{
    public class Statistic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonProperty("activeTorrentCount")]
        public int ActiveTorrentCount { get; set; }

        [JsonProperty("downloadSpeed")]
        public int downloadSpeed{ get; set; }

        [JsonProperty("pausedTorrentCount")]
        public int pausedTorrentCount{ get; set; }

        [JsonProperty("torrentCount")]
        public int torrentCount{ get; set; }

        [JsonProperty("uploadSpeed")]
        public int uploadSpeed{ get; set; }
   
        [JsonProperty("cumulative-stats")]
        public CommonStatistic CumulativeStats { get; set; }
 
        [JsonProperty("current-stats")]
        public CommonStatistic CurrentStats { get; set; }
    }

    public class CommonStatistic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonProperty("uploadedBytes")]
        public long uploadedBytes{ get; set; }
        
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
