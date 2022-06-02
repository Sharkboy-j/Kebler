﻿using System.ComponentModel;
using Newtonsoft.Json;

namespace Kebler.Transmission.Models
{
    public class TorrentAddResult 
    {
        public Enums.AddTorrentStatus Status = Enums.AddTorrentStatus.UnknownError;

        /// <summary>
        ///     Torrent ID
        /// </summary>
        [JsonProperty("id")]
        public uint ID { get; set; }

        /// <summary>
        ///     Torrent name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Torrent Hash
        /// </summary>
        [JsonProperty("hashString")]
        public string HashString { get; set; }

    }
}