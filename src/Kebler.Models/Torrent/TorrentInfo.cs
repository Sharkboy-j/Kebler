using System;
using System.ComponentModel;
using System.Reflection;
using Kebler.Models.Torrent.Attributes;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TorrentInfo : INotifyPropertyChanged, IComparable
    {
        public TorrentInfo(int id)
        {
            Id = id;
        }

        [JsonProperty(TorrentFields.ID)]
        [SetIgnore]
        public int Id { get; }

        [JsonProperty(TorrentFields.ADDED_DATE)]
        [SetIgnore]
        public int AddedDate { get; set; }

        [JsonProperty(TorrentFields.BANDWIDTH_PRIORITY)]
        public int BandwidthPriority { get; set; }

        [JsonProperty(TorrentFields.COMMENT)]
        public string Comment { get; set; }

        [JsonProperty(TorrentFields.CORRUPT_EVER)]
        public int CorruptEver { get; set; }

        [JsonProperty(TorrentFields.CREATOR)]
        public string Creator { get; set; }

        [JsonProperty(TorrentFields.DATE_CREATED)]
        [SetIgnore]
        public int DateCreated { get; set; }

        [JsonProperty(TorrentFields.DESIRED_AVAILABLE)]
        public long DesiredAvailable { get; set; }

        [JsonProperty(TorrentFields.DONE_DATE)]
        public int DoneDate { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_DIR)]
        public string DownloadDir { get; set; }

        [JsonProperty(TorrentFields.DOWNLOADED_EVER)]
        public string DownloadedEver { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_LIMIT)]
        public string DownloadLimit { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_LIMITED)]
        public string DownloadLimited { get; set; }

        [JsonProperty(TorrentFields.ERROR)]
        public int Error { get; set; }

        [JsonProperty(TorrentFields.ERROR_STRING)]
        public string ErrorString { get; set; }

        [JsonProperty(TorrentFields.ETA)]
        public int ETA { get; set; }

        [JsonProperty(TorrentFields.ETA_IDLE)]
        public int ETAIdle { get; set; }

        [JsonProperty(TorrentFields.FILES)]
        public TransmissionTorrentFiles[] Files { get; set; }

        [JsonProperty(TorrentFields.FILE_STATS)]
        public TransmissionTorrentFileStats[] FileStats { get; set; }

        [JsonProperty(TorrentFields.HASH_STRING)]
        public string HashString { get; set; }

        [JsonProperty(TorrentFields.HAVE_UNCHECKED)]
        public int HaveUnchecked { get; set; }

        [JsonProperty(TorrentFields.HAVE_VALID)]
        public long HaveValid { get; set; }

        [JsonProperty(TorrentFields.HONORS_SESSION_LIMITS)]
        public bool HonorsSessionLimits { get; set; }

        [JsonProperty(TorrentFields.IS_FINISHED)]
        public bool IsFinished { get; set; }

        [JsonProperty(TorrentFields.IS_PRIVATE)]
        public bool IsPrivate { get; set; }

        [JsonProperty(TorrentFields.IS_STALLED)]
        public bool IsStalled { get; set; }

        [JsonProperty(TorrentFields.LEFT_UNTIL_DONE)]
        public long LeftUntilDone { get; set; }

        [JsonProperty(TorrentFields.MAGNET_LINK)]
        public string MagnetLink { get; set; }

        [JsonProperty(TorrentFields.MANUAL_ANNOUNCE_TIME)]
        public int ManualAnnounceTime { get; set; }

        [JsonProperty(TorrentFields.MAX_CONNECTED_PEERS)]
        public int MaxConnectedPeers { get; set; }

        [JsonProperty(TorrentFields.METADATA_PERCENT_COMPLETE)]
        public double MetadataPercentComplete { get; set; }

        [JsonProperty(TorrentFields.NAME)]
        public string Name { get; set; }

        [JsonProperty(TorrentFields.PEER_LIMIT)]
        public int PeerLimit { get; set; }

        [JsonProperty(TorrentFields.PEERS)]
        public TransmissionTorrentPeers[] Peers { get; set; }

        [JsonProperty(TorrentFields.PEERS_CONNECTED)]
        public int PeersConnected { get; set; }

        [JsonProperty(TorrentFields.PEERS_FROM)]
        public TransmissionTorrentPeersFrom PeersFrom { get; set; }

        [JsonProperty(TorrentFields.PEERS_SENDING_TO_US)]
        public int PeersSendingToUs { get; set; }

        [JsonProperty(TorrentFields.PERCENT_DONE)]
        public double PercentDone { get; set; }

        [JsonProperty(TorrentFields.PIECES)]
        public string Pieces { get; set; }

        [JsonProperty(TorrentFields.PIECE_COUNT)]
        public int PieceCount { get; set; }

        [JsonProperty(TorrentFields.PIECE_SIZE)]
        public int PieceSize { get; set; }

        [JsonProperty(TorrentFields.PRIORITIES)]
        public int[] Priorities { get; set; }

        [JsonProperty(TorrentFields.QUEUE_POSITION)]
        public int QueuePosition { get; set; }

        [JsonProperty(TorrentFields.RATE_DOWNLOAD)]
        public int RateDownload { get; set; }

        [JsonProperty(TorrentFields.RATE_UPLOAD)]
        public int RateUpload { get; set; }

        [JsonProperty(TorrentFields.RECHECK)]
        public double RecheckProgress { get; set; }

        [JsonProperty(TorrentFields.SECONDS_DOWNLOADING)]
        public int SecondsDownloading { get; set; }

        [JsonProperty(TorrentFields.SECONDS_SEEDING)]
        public int SecondsSeeding { get; set; }

        [JsonProperty(TorrentFields.SEED_IDLE_LIMIT)]
        public int SeedIdleLimit { get; set; }

        [JsonProperty(TorrentFields.SEED_IDLE_MODE)]
        public int SeedIdleMode { get; set; }

        [JsonProperty(TorrentFields.SEED_RATIO_LIMIT)]
        public double SeedRatioLimit { get; set; }

        [JsonProperty(TorrentFields.SEED_RATIO_MODE)]
        public int SeedRatioMode { get; set; }

        [JsonProperty(TorrentFields.SIZE_WHEN_DONE)]
        public long SizeWhenDone { get; set; }

        [JsonProperty(TorrentFields.START_DATE)]
        public int StartDate { get; set; }

        /// <summary>
        /// <para>0: 'stopped'</para> 
        /// <para>1: 'check pending'</para> 
        /// <para>2: 'checking'</para> 
        /// <para>3: 'download pending'</para> 
        /// <para>4: 'downloading'</para> 
        /// <para>5: 'seed pending'</para> 
        /// <para>6: 'seeding' </para> 
        /// </summary>
        [JsonProperty(TorrentFields.STATUS)]
        public int Status { get; set; }

        [JsonProperty(TorrentFields.TRACKERS)]
        public TransmissionTorrentTrackers[] Trackers { get; set; }

        [JsonProperty(TorrentFields.TRACKER_STATS)]
        public TransmissionTorrentTrackerStats[] TrackerStats { get; set; }

        [JsonProperty(TorrentFields.TOTAL_SIZE)]
        public long TotalSize { get; set; }

        [JsonProperty(TorrentFields.TORRENT_FILE)]
        public string TorrentFile { get; set; }

        [JsonProperty(TorrentFields.UPLOADED_EVER)]
        public long UploadedEver { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_LIMIT)]
        public int UploadLimit { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_LIMITED)]
        public bool UploadLimited { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_RATIO)]
        public double UploadRatio { get; set; }

        [JsonProperty(TorrentFields.WANTED)]
        public bool[] Wanted { get; set; }

        [JsonProperty(TorrentFields.WEB_SEEDS)]
        public string[] Webseeds { get; set; }

        [JsonProperty(TorrentFields.WEB_SEEDS_SENDING_TO_US)]
        public int WebseedsSendingToUs { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Name}";
        }

        public void Set(string propertyName, object value)
        {
            var myType = typeof(TorrentInfo);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);

        }
        public object Get(string propertyName)
        {

            var myType = typeof(TorrentInfo);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }


        public override bool Equals(object obj)
        {
            if (obj != null && obj is TorrentInfo torrentInfo)
            {
                return torrentInfo.Id == Id &&
                    torrentInfo.HashString == HashString;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }

        public int CompareTo(object obj)
        {
            return Status.CompareTo(((TorrentInfo)obj).Status);
        }


        //~TorrentInfo()
        //{
        //    Comment = null;
        //    Creator = null;
        //    DownloadDir = null;
        //    DownloadedEver = null;
        //    DownloadLimit = null;
        //    DownloadLimited = null;
        //    ErrorString = null;
        //    Files = null;
        //    FileStats = null;
        //    HashString = null;
        //    MagnetLink = null;
        //    Name = null;
        //    Peers = null;
        //    PeersFrom = null;
        //    Pieces = null;
        //    Trackers = null;
        //    TrackerStats = null;
        //    TorrentFile = null;
        //    Webseeds = null;
        //    PropertyChanged = null;
        //}

    }


    //TODO: Separate "remove" and "active" torrents in "torrentsGet"
}
