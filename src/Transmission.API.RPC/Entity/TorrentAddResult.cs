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

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public int Name { get; set; }

        [JsonProperty("hashString")]
        public int Hash { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
