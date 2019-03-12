using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transmission.API.RPC.Common;
using Transmission.API.RPC.Entity;

namespace Transmission.API.RPC.Arguments
{
    public class SessionSettings : ArgumentsBase
    {
        /// <summary>
        /// Max global download speed (KBps)
        /// </summary>
        public int? AlternativeSpeedDown { get { return GetValue<int?>("alt-speed-down"); } set { this["alt-speed-down"] = value; } }

        /// <summary>
        /// True means use the alt speeds
        /// </summary>
        public bool? AlternativeSpeedEnabled { get { return GetValue<bool?>("alt-speed-enabled"); } set { this["alt-speed-enabled"] = value; } }

        /// <summary>
        /// When to turn on alt speeds (units: minutes after midnight)
        /// </summary>
        public int? AlternativeSpeedTimeBegin { get { return GetValue<int?>("alt-speed-time-begin"); } set { this["alt-speed-time-begin"] = value; } }

        /// <summary>
        /// True means the scheduled on/off times are used
        /// </summary>
        public bool? AlternativeSpeedTimeEnabled { get { return GetValue<bool?>("alt-speed-time-enabled"); } set { this["bandwidthPriority"] = value; } }

        /// <summary>
        /// When to turn off alt speeds
        /// </summary>
        public int? AlternativeSpeedTimeEnd { get { return GetValue<int?>("alt-speed-time-end"); } set { this["alt-speed-time-end"] = value; } }

        /// <summary>
        /// What day(s) to turn on alt speeds
        /// </summary>
        public int? AlternativeSpeedTimeDay { get { return GetValue<int?>("alt-speed-time-day"); } set { this["alt-speed-time-day"] = value; } }

        /// <summary>
        /// Max global upload speed (KBps)
        /// </summary>
        public int? AlternativeSpeedUp { get { return GetValue<int?>("alt-speed-up"); } set { this["alt-speed-up"] = value; } }

        /// <summary>
        /// Location of the blocklist to use for "blocklist-update"
        /// </summary>
        public string BlocklistURL { get { return GetValue<string>("blocklist-url"); } set { this["blocklist-url"] = value; } }

        /// <summary>
        /// True means enabled
        /// </summary>
        public bool? BlocklistEnabled { get { return GetValue<bool?>("blocklist-enabled"); } set { this["blocklist-enabled"] = value; } }

        /// <summary>
        /// Maximum size of the disk cache (MB)
        /// </summary>
        public int? CacheSizeMB { get { return GetValue<int?>("cache-size-mb"); } set { this["cache-size-mb"] = value; } }

        /// <summary>
        /// Default path to download torrents
        /// </summary>
        public string DownloadDirectory { get { return GetValue<string>("download-dir"); } set { this["download-dir"] = value; } }

        /// <summary>
        /// Max number of torrents to download at once (see download-queue-enabled)
        /// </summary>
        public int? DownloadQueueSize { get { return GetValue<int?>("download-queue-size"); } set { this["download-queue-size"] = value; } }

        /// <summary>
        /// If true, limit how many torrents can be downloaded at once
        /// </summary>
        public bool? DownloadQueueEnabled { get { return GetValue<bool?>("download-queue-enabled"); } set { this["download-queue-enabled"] = value; } }

        /// <summary>
        /// True means allow dht in public torrents
        /// </summary>
        public bool? DHTEnabled { get { return GetValue<bool?>("dht-enabled"); } set { this["dht-enabled"] = value; } }

        /// <summary>
        /// "required", "preferred", "tolerated"
        /// </summary>
        public string Encryption { get { return GetValue<string>("encryption"); } set { this["encryption"] = value; } }

        /// <summary>
        /// Torrents we're seeding will be stopped if they're idle for this long
        /// </summary>
        public int? IdleSeedingLimit { get { return GetValue<int?>("idle-seeding-limit"); } set { this["idle-seeding-limit"] = value; } }

        /// <summary>
        /// True if the seeding inactivity limit is honored by default
        /// </summary>
        public bool? IdleSeedingLimitEnabled { get { return GetValue<bool?>("idle-seeding-limit-enabled"); } set { this["idle-seeding-limit-enabled"] = value; } }

        /// <summary>
        /// Path for incomplete torrents, when enabled
        /// </summary>
        public string IncompleteDirectory { get { return GetValue<string>("incomplete-dir"); } set { this["incomplete-dir"] = value; } }

        /// <summary>
        /// True means keep torrents in incomplete-dir until done
        /// </summary>
        public bool? IncompleteDirectoryEnabled { get { return GetValue<bool?>("incomplete-dir-enabled"); } set { this["incomplete-dir-enabled"] = value; } }

        /// <summary>
        /// True means allow Local Peer Discovery in public torrents
        /// </summary>
        public bool? LPDEnabled { get { return GetValue<bool?>("lpd-enabled"); } set { this["lpd-enabled"] = value; } }

        /// <summary>
        /// Maximum global number of peers
        /// </summary>
        public int? PeerLimitGlobal { get { return GetValue<int?>("peer-limit-global"); } set { this["peer-limit-global"] = value; } }

        /// <summary>
        /// Maximum global number of peers
        /// </summary>
        public int? PeerLimitPerTorrent { get { return GetValue<int?>("peer-limit-per-torrent"); } set { this["peer-limit-per-torrent"] = value; } }

        /// <summary>
        /// True means allow pex in public torrents
        /// </summary>
        public bool? PexEnabled { get { return GetValue<bool?>("pex-enabled"); } set { this["pex-enabled"] = value; } }

        /// <summary>
        /// Port number
        /// </summary>
        public int? PeerPort { get { return GetValue<int?>("peer-port"); } set { this["peer-port"] = value; } }

        /// <summary>
        /// True means pick a random peer port on launch
        /// </summary>
        public bool? PeerPortRandomOnStart { get { return GetValue<bool?>("peer-port-random-on-start"); } set { this["peer-port-random-on-start"] = value; } }

        /// <summary>
        /// true means enabled
        /// </summary>
        public bool? PortForwardingEnabled { get { return GetValue<bool?>("port-forwarding-enabled"); } set { this["port-forwarding-enabled"] = value; } }

        /// <summary>
        /// Whether or not to consider idle torrents as stalled
        /// </summary>
        public bool? QueueStalledEnabled { get { return GetValue<bool?>("queue-stalled-enabled"); } set { this["queue-stalled-enabled"] = value; } }

        /// <summary>
        /// Torrents that are idle for N minuets aren't counted toward seed-queue-size or download-queue-size
        /// </summary>
        public int? QueueStalledMinutes { get { return GetValue<int?>("queue-stalled-minutes"); } set { this["queue-stalled-minutes"] = value; } }

        /// <summary>
        /// True means append ".part" to incomplete files
        /// </summary>
        public bool? RenamePartialFiles { get { return GetValue<bool?>("rename-partial-files"); } set { this["rename-partial-files"] = value; } }

        /// <summary>
        /// Filename of the script to run
        /// </summary>
        public string ScriptTorrentDoneFilename { get { return GetValue<string>("script-torrent-done-filename"); } set { this["script-torrent-done-filename"] = value; } }

        /// <summary>
        /// Whether or not to call the "done" script
        /// </summary>
        public bool? ScriptTorrentDoneEnabled { get { return GetValue<bool?>("script-torrent-done-enabled"); } set { this["script-torrent-done-enabled"] = value; } }

        /// <summary>
        /// The default seed ratio for torrents to use
        /// </summary>
        public double? SeedRatioLimit { get { return GetValue<int?>("seedRatioLimit"); } set { this["seedRatioLimit"] = value; } }

        /// <summary>
        /// True if seedRatioLimit is honored by default
        /// </summary>
        public bool? SeedRatioLimited { get { return GetValue<bool?>("seedRatioLimited"); } set { this["seedRatioLimited"] = value; } }

        /// <summary>
        /// Max number of torrents to uploaded at once (see seed-queue-enabled)
        /// </summary>
        public int? SeedQueueSize { get { return GetValue<int?>("seed-queue-size"); } set { this["seed-queue-size"] = value; } }

        /// <summary>
        /// If true, limit how many torrents can be uploaded at once
        /// </summary>
        public bool? SeedQueueEnabled { get { return GetValue<bool?>("seed-queue-enabled"); } set { this["seed-queue-enabled"] = value; } }

        /// <summary>
        /// Max global download speed (KBps)
        /// </summary>
        public int? SpeedLimitDown { get { return GetValue<int?>("speed-limit-down"); } set { this["speed-limit-down"] = value; } }

        /// <summary>
        /// True means enabled
        /// </summary>
        public bool? SpeedLimitDownEnabled { get { return GetValue<bool?>("speed-limit-down-enabled"); } set { this["speed-limit-down-enabled"] = value; } }

        /// <summary>
        ///  max global upload speed (KBps)
        /// </summary>
        public int? SpeedLimitUp { get { return GetValue<int?>("speed-limit-up"); } set { this["speed-limit-up"] = value; } }

        /// <summary>
        /// True means enabled
        /// </summary>
        public bool? SpeedLimitUpEnabled { get { return GetValue<bool?>("speed-limit-up-enabled"); } set { this["speed-limit-up-enabled"] = value; } }

        /// <summary>
        /// True means added torrents will be started right away
        /// </summary>
        public bool? StartAddedTorrents { get { return GetValue<bool?>("start-added-torrents"); } set { this["start-added-torrents"] = value; } }

        /// <summary>
        /// True means the .torrent file of added torrents will be deleted
        /// </summary>
        public bool? TrashOriginalTorrentFiles { get { return GetValue<bool?>("trash-original-torrent-files"); } set { this["trash-original-torrent-files"] = value; } }

        public Units Units { get { return GetValue<Units>("units"); } set { this["units"] = value; } }

        /// <summary>
        /// True means allow utp
        /// </summary>
        public bool? UtpEnabled { get { return GetValue<bool?> ("utp-enabled"); } set { this["utp-enabled"] = value; } }

    }
}
