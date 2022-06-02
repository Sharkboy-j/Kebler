using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace Kebler.Transmission.Models
{
    public class TransmissionTorrentFiles
    {
        [JsonConstructor]
        public TransmissionTorrentFiles()
        {
        }

        public TransmissionTorrentFiles(long pLen, params string[] fullPath)
        {
            Length = pLen;
            Name = Path.Combine(fullPath);
        }

        [JsonProperty("bytesCompleted")]
        public long BytesCompleted { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public string[] NameParts => Name.Split('/');

    }
}