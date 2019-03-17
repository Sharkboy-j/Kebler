using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmission.API.RPC.Common;

namespace Transmission.API.RPC.Entity
{
    /// <summary>
    /// Information about the torrent file, that will be added
    /// </summary>
    public class NewTorrent : ArgumentsBase
    {
        /// <summary>
        /// Pointer to a string of one or more cookies.
        /// </summary>
        public string Cookies { get { return GetValue<string>("cookies"); } set { this["cookies"] = value; } }

        /// <summary>
        /// Path to download the torrent to
        /// </summary>
        public string DownloadDirectory { get { return GetValue<string>("download-dir"); } set { this["download-dir"] = value; } }

        /// <summary>
		/// filename (relative to the server) or URL of the .torrent file (Priority than the metadata)
        /// </summary>
        public string Filename { get { return GetValue<string>("filename"); } set { this["filename"] = value; } }

        /// <summary>
        /// base64-encoded .torrent content
        /// </summary>
        public string Metainfo { get { return GetValue<string>("metainfo"); } set { this["metainfo"] = value; } }

        /// <summary>
        /// if true, don't start the torrent
        /// </summary>
        public bool Paused { get { return GetValue<bool>("metainfo"); } set { this["paused"] = value; } }

        /// <summary>
        /// maximum number of peers
        /// </summary>
        public int? PeerLimit { get { return GetValue<int?>("metainfo"); } set { this["peer-limit"] = value; } }

        /// <summary>
        /// Torrent's bandwidth priority
        /// </summary>
        public int? BandwidthPriority { get { return GetValue<int?>("metainfo"); } set { this["bandwidthPriority"] = value; } }

        /// <summary>
        /// Indices of file(s) to download
        /// </summary>
        public int[] FilesWanted { get { return GetValue<int[]>("metainfo"); } set { this["files-wanted"] = value; } }

        /// <summary>
        /// Indices of file(s) to download
        /// </summary>
        public int[] FilesUnwanted { get { return GetValue<int[]>("metainfo"); } set { this["files-unwanted"] = value; } }

        /// <summary>
        /// Indices of high-priority file(s)
        /// </summary>
        public int[] PriorityHigh { get { return GetValue<int[]>("metainfo"); } set { this["priority-high"] = value; } }

        /// <summary>
        /// Indices of low-priority file(s)
        /// </summary>
        public int[] PriorityLow { get { return GetValue<int[]>("metainfo"); } set { this["priority-low"] = value; } }

        /// <summary>
        /// Indices of normal-priority file(s)
        /// </summary>
        public int[] PriorityNormal { get { return GetValue<int[]>("metainfo"); } set { this["priority-normal"] = value; } }
    }
}
