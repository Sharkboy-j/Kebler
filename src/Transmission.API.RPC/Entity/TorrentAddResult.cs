namespace Transmission.API.RPC.Entity
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    public class TorrentAddResult : INotifyPropertyChanged
    {
        public TorrentAddResult(Enums.AddTorrentStatus status)
        {
            Status = status;
        }

        /// <summary>
		/// Torrent ID
		/// </summary>
		[JsonProperty("id")]
        public int ID { get; set; }

        /// <summary>
        /// Torrent name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Torrent Hash
        /// </summary>
        [JsonProperty("hashString")]
        public string HashString { get; set; }

        public Enums.AddTorrentStatus Status;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
