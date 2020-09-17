namespace Kebler.Models.Torrent.Args
{
    public class TorrentSettings : ArgumentsBase
    {
        /// <summary>
        /// This torrent's bandwidth tr_priority_t
        /// </summary>
        public int? BandwidthPriority
        {
            get => GetValue<int?>("bandwidthPriority");
            set => this["bandwidthPriority"] = value;
        }

        /// <summary>
        /// Maximum download speed (KBps)
        /// </summary>
        public long? DownloadLimit
        {
            get => GetValue<int?>("downloadLimit");
            set => this["downloadLimit"] = value;
        }

        /// <summary>
        /// Download limit is honored
        /// </summary>
        public bool? DownloadLimited
        {
            get => GetValue<bool?>("downloadLimited");
            set => this["downloadLimited"] = value;
        }

        /// <summary>
        /// Session upload limits are honored
        /// </summary>
        public bool? HonorsSessionLimits
        {
            get => GetValue<bool?>("honorsSessionLimits");
            set => this["honorsSessionLimits"] = value;
        }

        /// <summary>
        /// Torrent id array
        /// </summary>
        public uint[] IDs
        {
            get => GetValue<uint[]>("ids");
            set => this["ids"] = value;
        }

        /// <summary>
        /// New location of the torrent's content
        /// </summary>
        public string Location
        {
            get => GetValue<string>("location");
            set => this["location"] = value;
        }

        /// <summary>
        /// Maximum number of peers
        /// </summary>
        public int? PeerLimit
        {
            get => GetValue<int?>("peer-limit");
            set => this["peer-limit"] = value;
        }

        /// <summary>
        /// Position of this torrent in its queue [0...n)
        /// </summary>
        public int? QueuePosition
        {
            get => GetValue<int?>("queuePosition");
            set => this["queuePosition"] = value;
        }

        /// <summary>
        /// Torrent-level number of minutes of seeding inactivity
        /// </summary>
        public int? SeedIdleLimit
        {
            get => GetValue<int?>("seedIdleLimit");
            set => this["seedIdleLimit"] = value;
        }

        /// <summary>
        /// Which seeding inactivity to use
        /// </summary>
        public int? SeedIdleMode
        {
            get => GetValue<int?>("seedIdleMode");
            set => this["seedIdleMode"] = value;
        }

        /// <summary>
        /// Torrent-level seeding ratio
        /// </summary>
        public double? SeedRatioLimit
        {
            get => GetValue<double?>("seedRatioLimit");
            set => this["seedRatioLimit"] = value;
        }

        /// <summary>
        /// Which ratio to use. 
        /// </summary>
        public int? SeedRatioMode
        {
            get => GetValue<int?>("seedRatioMode");
            set => this["seedRatioMode"] = value;
        }

        /// <summary>
        /// Maximum upload speed (KBps)
        /// </summary>
        public long? UploadLimit
        {
            get => GetValue<int?>("uploadLimit");
            set => this["uploadLimit"] = value;
        }

        /// <summary>
        /// Upload limit is honored
        /// </summary>
        public bool? UploadLimited
        {
            get => GetValue<bool?>("uploadLimited");
            set => this["uploadLimited"] = value;
        }

        /// <summary>
        /// Strings of announce URLs to add
        /// </summary>
		public string[] TrackerAdd
        {
            get => GetValue<string[]>("trackerAdd");
            set => this["trackerAdd"] = value;
        }

        /// <summary>
        /// Ids of trackers to remove
        /// </summary>
		public int[] TrackerRemove
        {
            get => GetValue<int[]>("trackerRemove");
            set => this["trackerRemove"] = value;
        }

        public uint[] FilesWanted
        {
            get => GetValue<uint[]>("files-wanted");
            set => this["files-wanted"] = value;
        }

        public uint[] FilesUnwanted
        {
            get => GetValue<uint[]>("files-unwanted");
            set => this["files-unwanted"] = value;
        }

        //"trackerReplace"      | array      pairs of <trackerId/new announce URLs>
        //public [] trackerReplace;

        //"priority-high"       | array      indices of high-priority file(s)
        //public [] PriorityHigh;

        //"priority-low"        | array      indices of low-priority file(s)
        //public [] PriorityLow;

        //"priority-normal"     | array      indices of normal-priority file(s)
        //public [] PriorityNormal;


        public object Get(string propertyName)
        {

            var myType = typeof(TorrentSettings);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }
        public void Set(string propertyName, object value)
        {
            var myType = typeof(TorrentInfo);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);

        }

    }
}
