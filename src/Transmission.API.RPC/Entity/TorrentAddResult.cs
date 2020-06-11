using Newtonsoft.Json;
using System.ComponentModel;
using static Transmission.API.RPC.Entity.Enums;

namespace Transmission.API.RPC.Entity
{
    public class TorrentAddResult : INotifyPropertyChanged
    {
        public TorrentAddResult()
        {
        }

        public TorrentAddResult(AddResult result)
        {
            Result = result;
        }

        public AddResult Result { get; set; }




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



        //[JsonProperty("id")]
        //public int Id { get; set; }

        //[JsonProperty("name")]
        //public int Name { get; set; }

        //[JsonProperty("hashString")]
        //public string Hash { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
