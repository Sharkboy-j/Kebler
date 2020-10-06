using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentTrackers
    {
        [JsonProperty("announce")]
        public string announce { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("scrape")]
        public string Scrape { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is TransmissionTorrentTrackers tracker)
            {
                if (tracker.announce.ToLower().Equals(this.announce.ToLower())) ;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return announce.ToLower().GetHashCode();
        }
    }
}