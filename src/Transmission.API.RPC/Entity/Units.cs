using System.ComponentModel;
using Newtonsoft.Json;

namespace Transmission.API.RPC.Entity
{
    public class Units : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [JsonProperty("speed-units")]
        public string[] SpeedUnits { get; set; }

        [JsonProperty("speed-bytes")]
        public int? SpeedBytes { get; set; }

        [JsonProperty("size-units")]
        public string[] SizeUnits { get; set; }

        [JsonProperty("size-bytes")]
        public int? SizeBytes { get; set; }

        [JsonProperty("memory-units")]
        public string[] MemoryUnits { get; set; }

        [JsonProperty("memory-bytes")]
        public int? MemoryBytes { get; set; }
    }
}
