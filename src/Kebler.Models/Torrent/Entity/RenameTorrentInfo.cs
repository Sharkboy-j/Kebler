using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Entity
{
	/// <summary>
	/// Rename torrent result information
	/// </summary>
	public class RenameTorrentInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// The torrent's unique Id.
        /// </summary>
        [JsonProperty("id")]
        public int ID { get; set; }

		/// <summary>
		/// File path.
		/// </summary>
		[JsonProperty("path")]
		public string Path { get; set; }

		/// <summary>
		/// File name.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
