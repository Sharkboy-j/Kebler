using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmission.API.RPC.Entity
{
	/// <summary>
    /// Rename torrent result information
    /// </summary>
	public class RenameTorrentInfo
	{ 
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
