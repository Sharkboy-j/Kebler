using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Entity
{
    public class TorrentAddResult : INotifyPropertyChanged
    {
        /// <summary>
		/// Torrent ID
		/// </summary>
		[JsonProperty("id")]
        public uint ID { get; set; }

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

        public Enums.AddTorrentStatus Status = Enums.AddTorrentStatus.UnknownError;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
