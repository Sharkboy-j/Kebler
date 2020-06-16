using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentFileStats : INotifyPropertyChanged
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted { get; set; }

        [JsonProperty("wanted")]
        public bool Wanted { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        //~TransmissionTorrentFileStats()
        //{
        //    PropertyChanged = null;
        //}
    }
}
