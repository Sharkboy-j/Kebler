using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Transmission.Models
{
    /// <summary>
    ///     Contains arrays of torrents and removed torrents
    /// </summary>
    public class TransmissionTorrents : ICloneable
    {
        /// <summary>
        ///     Array of torrents
        /// </summary>
        [JsonProperty("torrents")]
        public TorrentInfo[] Torrents { get; set; }

        /// <summary>
        ///     Array of torrent-id numbers of recently-removed torrents
        /// </summary>
        [JsonProperty("removed")]
        public TorrentInfo[] Removed { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}