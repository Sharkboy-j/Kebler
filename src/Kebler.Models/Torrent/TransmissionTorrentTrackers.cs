using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentTrackers : INotifyPropertyChanged
    {
        [JsonProperty("announce")]
        public string announce { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("scrape")]
        public string Scrape { get; set; }

        [JsonProperty("tier")]
        public int Tier { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        //~TransmissionTorrentTrackers()
        //{
        //    announce = null;
        //    Scrape = null;
        //    PropertyChanged = null;
        //}
    }
}