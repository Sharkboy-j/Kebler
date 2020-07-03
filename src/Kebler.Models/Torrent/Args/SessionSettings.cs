using Kebler.Models.Torrent.Entity;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent.Args
{
    public class SessionSettings : ArgumentsBase
    {
        /// <summary>
        /// Max global download speed (KBps)
        /// </summary>
        [JsonProperty("alt-speed-down")]
        public long AlternativeSpeedDown
        {
            get => GetValue<long>("alt-speed-down");
            set => this["alt-speed-down"] = value;
        }

        /// <summary>
        /// True means use the alt speeds
        /// </summary>
        [JsonProperty("alt-speed-enabled")]
        public bool? AlternativeSpeedEnabled
        {
            get => GetValue<bool?>("alt-speed-enabled");
            set => this["alt-speed-enabled"] = value;
        }

        /// <summary>
        /// When to turn on alt speeds (units: minutes after midnight)
        /// </summary>
        [JsonProperty("alt-speed-time-begin")]
        public int? AlternativeSpeedTimeBegin
        {
            get => GetValue<int?>("alt-speed-time-begin");
            set => this["alt-speed-time-begin"] = value;
        }

        /// <summary>
        /// True means the scheduled on/off times are used
        /// </summary>
        [JsonProperty("alt-speed-time-enabled")]
        public bool? AlternativeSpeedTimeEnabled
        {
            get => GetValue<bool?>("alt-speed-time-enabled");
            set => this["bandwidthPriority"] = value;
        }

        /// <summary>
        /// When to turn off alt speeds
        /// </summary>
        [JsonProperty("alt-speed-time-end")]
        public int? AlternativeSpeedTimeEnd
        {
            get => GetValue<int?>("alt-speed-time-end");
            set => this["alt-speed-time-end"] = value;
        }

        /// <summary>
        /// What day(s) to turn on alt speeds
        /// </summary>
        [JsonProperty("alt-speed-time-day")]
        public int? AlternativeSpeedTimeDay
        {
            get => GetValue<int?>("alt-speed-time-day");
            set => this["alt-speed-time-day"] = value;
        }

        /// <summary>
        /// Max global upload speed (KBps)
        /// </summary>
        [JsonProperty("alt-speed-up")]
        public long AlternativeSpeedUp
        {
            get => GetValue<long>("alt-speed-up");
            set => this["alt-speed-up"] = value;
        }

        /// <summary>
        /// Location of the blocklist to use for "blocklist-update"
        /// </summary>
        [JsonProperty("blocklist-url")]
        public string BlocklistURL
        {
            get => GetValue<string>("blocklist-url");
            set => this["blocklist-url"] = value;
        }

        /// <summary>
        /// True means enabled
        /// </summary>
        [JsonProperty("blocklist-enabled")]
        public bool? BlocklistEnabled
        {
            get => GetValue<bool?>("blocklist-enabled");
            set => this["blocklist-enabled"] = value;
        }

        /// <summary>
        /// Maximum size of the disk cache (MB)
        /// </summary>
        [JsonProperty("cache-size-mb")]
        public int? CacheSizeMB
        {
            get => GetValue<int?>("cache-size-mb");
            set => this["cache-size-mb"] = value;
        }

        /// <summary>
        /// Default path to download torrents
        /// </summary>
        [JsonProperty("download-dir")]
        public string DownloadDirectory
        {
            get => GetValue<string>("download-dir");
            set => this["download-dir"] = value;
        }

        /// <summary>
        /// Max number of torrents to download at once (see download-queue-enabled)
        /// </summary>
        [JsonProperty("download-queue-size")]
        public int? DownloadQueueSize
        {
            get => GetValue<int?>("download-queue-size");
            set => this["download-queue-size"] = value;
        }

        /// <summary>
        /// If true, limit how many torrents can be downloaded at once
        /// </summary>
        [JsonProperty("download-queue-enabled")]
        public bool? DownloadQueueEnabled
        {
            get => GetValue<bool?>("download-queue-enabled");
            set => this["download-queue-enabled"] = value;
        }

        /// <summary>
        /// True means allow dht in public torrents
        /// </summary>
        [JsonProperty("dht-enabled")]
        public bool? DHTEnabled
        {
            get => GetValue<bool?>("dht-enabled");
            set => this["dht-enabled"] = value;
        }

        /// <summary>
        /// "required", "preferred", "tolerated"
        /// </summary>
        [JsonProperty("encryption")]
        public string Encryption
        {
            get => GetValue<string>("encryption");
            set => this["encryption"] = value;
        }

        /// <summary>
        /// Torrents we're seeding will be stopped if they're idle for this long
        /// </summary>
        [JsonProperty("idle-seeding-limit")]
        public int? IdleSeedingLimit
        {
            get => GetValue<int?>("idle-seeding-limit");
            set => this["idle-seeding-limit"] = value;
        }

        /// <summary>
        /// True if the seeding inactivity limit is honored by default
        /// </summary>
        [JsonProperty("idle-seeding-limit-enabled")]
        public bool? IdleSeedingLimitEnabled
        {
            get => GetValue<bool?>("idle-seeding-limit-enabled");
            set => this["idle-seeding-limit-enabled"] = value;
        }

        /// <summary>
        /// Path for incomplete torrents, when enabled
        /// </summary>
        [JsonProperty("incomplete-dir")]
        public string IncompleteDirectory
        {
            get => GetValue<string>("incomplete-dir");
            set => this["incomplete-dir"] = value;
        }

        /// <summary>
        /// True means keep torrents in incomplete-dir until done
        /// </summary>
        [JsonProperty("incomplete-dir-enabled")]
        public bool? IncompleteDirectoryEnabled
        {
            get => GetValue<bool?>("incomplete-dir-enabled");
            set => this["incomplete-dir-enabled"] = value;
        }

        /// <summary>
        /// True means allow Local Peer Discovery in public torrents
        /// </summary>
        [JsonProperty("lpd-enabled")]
        public bool? LPDEnabled
        {
            get => GetValue<bool?>("lpd-enabled");
            set => this["lpd-enabled"] = value;
        }

        /// <summary>
        /// Maximum global number of peers
        /// </summary>
        [JsonProperty("peer-limit-global")]
        public int? PeerLimitGlobal
        {
            get => GetValue<int?>("peer-limit-global");
            set => this["peer-limit-global"] = value;
        }

        /// <summary>
        /// Maximum global number of peers
        /// </summary>
        [JsonProperty("peer-limit-per-torrent")]
        public int? PeerLimitPerTorrent
        {
            get => GetValue<int?>("peer-limit-per-torrent");
            set => this["peer-limit-per-torrent"] = value;
        }

        /// <summary>
        /// True means allow pex in public torrents
        /// </summary>
        [JsonProperty("pex-enabled")]
        public bool? PexEnabled
        {
            get => GetValue<bool?>("pex-enabled");
            set => this["pex-enabled"] = value;
        }

        /// <summary>
        /// Port number
        /// </summary>
        [JsonProperty("peer-port")]
        public int? PeerPort
        {
            get => GetValue<int?>("peer-port");
            set => this["peer-port"] = value;
        }

        /// <summary>
        /// True means pick a random peer port on launch
        /// </summary>
        [JsonProperty("peer-port-random-on-start")]
        public bool? PeerPortRandomOnStart
        {
            get => GetValue<bool?>("peer-port-random-on-start");
            set => this["peer-port-random-on-start"] = value;
        }

        /// <summary>
        /// true means enabled
        /// </summary>
        [JsonProperty("port-forwarding-enabled")]
        public bool? PortForwardingEnabled
        {
            get => GetValue<bool?>("port-forwarding-enabled");
            set => this["port-forwarding-enabled"] = value;
        }

        /// <summary>
        /// Whether or not to consider idle torrents as stalled
        /// </summary>
        [JsonProperty("queue-stalled-enabled")]
        public bool? QueueStalledEnabled
        {
            get => GetValue<bool?>("queue-stalled-enabled");
            set => this["queue-stalled-enabled"] = value;
        }

        /// <summary>
        /// Torrents that are idle for N minuets aren't counted toward seed-queue-size or download-queue-size
        /// </summary>
        [JsonProperty("queue-stalled-minutes")]
        public int? QueueStalledMinutes
        {
            get => GetValue<int?>("queue-stalled-minutes");
            set => this["queue-stalled-minutes"] = value;
        }

        /// <summary>
        /// True means append ".part" to incomplete files
        /// </summary>
        [JsonProperty("rename-partial-files")]
        public bool? RenamePartialFiles
        {
            get => GetValue<bool?>("rename-partial-files");
            set => this["rename-partial-files"] = value;
        }

        /// <summary>
        /// Filename of the script to run
        /// </summary>
        [JsonProperty("script-torrent-done-filename")]
        public string ScriptTorrentDoneFilename
        {
            get => GetValue<string>("script-torrent-done-filename");
            set => this["script-torrent-done-filename"] = value;
        }

        /// <summary>
        /// Whether or not to call the "done" script
        /// </summary>
        [JsonProperty("script-torrent-done-enabled")]
        public bool? ScriptTorrentDoneEnabled
        {
            get => GetValue<bool?>("script-torrent-done-enabled");
            set => this["script-torrent-done-enabled"] = value;
        }

        /// <summary>
        /// The default seed ratio for torrents to use
        /// </summary>
        [JsonProperty("seedRatioLimit")]
        public double? SeedRatioLimit
        {
            get => GetValue<double?>("seedRatioLimit");
            set => this["seedRatioLimit"] = value;
        }

        /// <summary>
        /// True if seedRatioLimit is honored by default
        /// </summary>
        [JsonProperty("seedRatioLimited")]
        public bool? SeedRatioLimited
        {
            get => GetValue<bool?>("seedRatioLimited");
            set => this["seedRatioLimited"] = value;
        }

        /// <summary>
        /// Max number of torrents to uploaded at once (see seed-queue-enabled)
        /// </summary>
        [JsonProperty("seed-queue-size")]
        public int? SeedQueueSize
        {
            get => GetValue<int?>("seed-queue-size");
            set => this["seed-queue-size"] = value;
        }

        /// <summary>
        /// If true, limit how many torrents can be uploaded at once
        /// </summary>
        [JsonProperty("seed-queue-enabled")]
        public bool? SeedQueueEnabled
        {
            get => GetValue<bool?>("seed-queue-enabled");
            set => this["seed-queue-enabled"] = value;
        }

        /// <summary>
        /// Max global download speed (KBps)
        /// </summary>
        [JsonProperty("speed-limit-down")]
        public int? SpeedLimitDown
        {
            get => GetValue<int?>("speed-limit-down");
            set => this["speed-limit-down"] = value;
        }

        /// <summary>
        /// True means enabled
        /// </summary>
        [JsonProperty("speed-limit-down-enabled")]
        public bool? SpeedLimitDownEnabled
        {
            get => GetValue<bool?>("speed-limit-down-enabled");
            set => this["speed-limit-down-enabled"] = value;
        }

        /// <summary>
        ///  max global upload speed (KBps)
        /// </summary>
        [JsonProperty("speed-limit-up")]
        public int? SpeedLimitUp
        {
            get => GetValue<int?>("speed-limit-up");
            set => this["speed-limit-up"] = value;
        }

        /// <summary>
        /// True means enabled
        /// </summary>
        [JsonProperty("speed-limit-up-enabled")]
        public bool? SpeedLimitUpEnabled
        {
            get => GetValue<bool?>("speed-limit-up-enabled");
            set => this["speed-limit-up-enabled"] = value;
        }

        /// <summary>
        /// True means added torrents will be started right away
        /// </summary>
        [JsonProperty("start-added-torrents")]
        public bool? StartAddedTorrents
        {
            get => GetValue<bool?>("start-added-torrents");
            set => this["start-added-torrents"] = value;
        }

        /// <summary>
        /// True means the .torrent file of added torrents will be deleted
        /// </summary>
        [JsonProperty("trash-original-torrent-files")]
        public bool? TrashOriginalTorrentFiles
        {
            get => GetValue<bool?>("trash-original-torrent-files");
            set => this["trash-original-torrent-files"] = value;
        }

        [JsonProperty("units")]
        public Units Units
        {
            get => GetValue<Units>("units");
            set => this["units"] = value;
        }

        /// <summary>
        /// True means allow utp
        /// </summary>
        [JsonProperty("utp-enabled")]
        public bool? UtpEnabled
        {
            get => GetValue<bool?>("utp-enabled");
            set => this["utp-enabled"] = value;
        }
        public object Get(string propertyName)
        {

            var myType = typeof(SessionSettings);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }
        public void Set(string propertyName, object value)
        {
            var myType = typeof(SessionSettings);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);

        }
    }
}
