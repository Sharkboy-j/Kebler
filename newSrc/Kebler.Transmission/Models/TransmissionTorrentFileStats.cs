using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Transmission.Models
{
    public class TransmissionTorrentFileStats 
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted { get; set; }

        [JsonProperty("wanted")]
        public bool Wanted { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }


        //~TransmissionTorrentFileStats()
        //{
        //    PropertyChanged = null;
        //}
    }
}