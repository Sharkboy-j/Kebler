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
#nullable enable
        public override bool Equals(object? obj)
        {
            if (obj is TransmissionTorrentTrackers tracker)
            {
                return tracker.announce.ToLower().Equals(this.announce.ToLower());
            }
            return false;
        }
#nullable disable
        public override int GetHashCode()
        {
            return announce.ToLower().GetHashCode();
        }
    }
}