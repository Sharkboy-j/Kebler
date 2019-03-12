using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmission.API.RPC.Entity
{
    public class Statistic
    {
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

    public class CommonStatistic
    {
        [JsonProperty("uploadedBytes")]
        public double uploadedBytes{ get; set; }
        
        [JsonProperty("downloadedBytes")]
        public double DownloadedBytes{ get; set; }

        [JsonProperty("filesAdded")]
        public int FilesAdded{ get; set; }

        [JsonProperty("SessionCount")]
        public int SessionCount{ get; set; }

        [JsonProperty("SecondsActive")]
        public int SecondsActive{ get; set; }
    }
}
