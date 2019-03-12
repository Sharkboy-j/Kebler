using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transmission.API.RPC.Entity
{
    /// <summary>
    /// Torrent information
    /// </summary>
    public class TorrentInfo
    {
        /// <summary>
        /// The torrent's unique Id.
        /// </summary>
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("addedDate")]
        public int AddedDate { get; set; }

        [JsonProperty("bandwidthPriority")]
        public int BandwidthPriority { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("corruptEver")]
        public int CorruptEver { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("dateCreated")]
        public int DateCreated { get; set; }

        [JsonProperty("desiredAvailable")]
        public long DesiredAvailable { get; set; }

        [JsonProperty("doneDate")]
        public int DoneDate { get; set; }

        [JsonProperty("downloadDir")]
        public string DownloadDir { get; set; }

        [JsonProperty("downloadedEver")]
        public string DownloadedEver { get; set; }

        [JsonProperty("downloadLimit")]
        public string DownloadLimit { get; set; }

        [JsonProperty("downloadLimited")]
        public string DownloadLimited { get; set; }

        [JsonProperty("error")]
        public int Error { get; set; }

        [JsonProperty("ErrorString")]
        public string ErrorString { get; set; }

        [JsonProperty("eta")]
        public int ETA { get; set; }

        [JsonProperty("etaIdle")]
        public int ETAIdle { get; set; }

        [JsonProperty("files")]
        public TransmissionTorrentFiles[] Files { get; set; }

        [JsonProperty("fileStats")]
        public TransmissionTorrentFileStats[] FileStats { get; set; }

        [JsonProperty("hashString")]
        public string HashString { get; set; }

        [JsonProperty("haveUnchecked")]
        public int HaveUnchecked { get; set; }

        [JsonProperty("haveValid")]
        public long HaveValid { get; set; }

        [JsonProperty("honorsSessionLimits")]
        public bool HonorsSessionLimits { get; set; }

        [JsonProperty("isFinished")]
        public bool IsFinished { get; set; }

        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty("isStalled")]
        public bool IsStalled { get; set; }

        [JsonProperty("leftUntilDone")]
        public long LeftUntilDone { get; set; }

        [JsonProperty("MagnetLink")]
        public string MagnetLink { get; set; }

        [JsonProperty("manualAnnounceTime")]
        public int ManualAnnounceTime { get; set; }

        [JsonProperty("maxConnectedPeers")]
        public int MaxConnectedPeers { get; set; }

        [JsonProperty("metadataPercentComplete")]
        public double MetadataPercentComplete { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("peer-limit")]
        public int PeerLimit { get; set; }

        [JsonProperty("peers")]
        public TransmissionTorrentPeers[] Peers { get; set; }

        [JsonProperty("peersConnected")]
        public int PeersConnected { get; set; }

        [JsonProperty("peersFrom")]
        public TransmissionTorrentPeersFrom PeersFrom { get; set; }

        [JsonProperty("peersSendingToUs")]
        public int PeersSendingToUs { get; set; }

        [JsonProperty("percentDone")]
        public double PercentDone { get; set; }

        [JsonProperty("pieces")]
        public string Pieces { get; set; }

        [JsonProperty("pieceCount")]
        public int PieceCount { get; set; }

        [JsonProperty("PieceSize")]
        public int PieceSize { get; set; }

        [JsonProperty("priorities")]
        public int[] Priorities { get; set; }

        [JsonProperty("queuePosition")]
        public int QueuePosition { get; set; }

        [JsonProperty("rateDownload")]
        public int RateDownload { get; set; }

        [JsonProperty("rateUpload")]
        public int RateUpload { get; set; }

        [JsonProperty("recheckProgress")]
        public double RecheckProgress { get; set; }

        [JsonProperty("secondsDownloading")]
        public int SecondsDownloading { get; set; }

        [JsonProperty("secondsSeeding")]
        public int SecondsSeeding { get; set; }

        [JsonProperty("seedIdleLimit")]
        public int SeedIdleLimit { get; set; }

        [JsonProperty("SeedIdleMode")]
        public int SeedIdleMode { get; set; }

        [JsonProperty("seedRatioLimit")]
        public double SeedRatioLimit { get; set; }

        [JsonProperty("SeedRatioMode")]
        public int SeedRatioMode { get; set; }

        [JsonProperty("SizeWhenDone")]
        public long SizeWhenDone { get; set; }

        [JsonProperty("startDate")]
        public int StartDate { get; set; }

        [JsonProperty("Status")]
        public int Status { get; set; }

		[JsonProperty("trackers")]
        public TransmissionTorrentTrackers[] Trackers { get; set; }

        [JsonProperty("trackerStats")]
        TransmissionTorrentTrackerStats[] TrackerStats { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("torrentFile")]
        public string TorrentFile { get; set; }

        [JsonProperty("uploadedEver")]
        public long UploadedEver { get; set; }

        [JsonProperty("uploadLimit")]
        public int UploadLimit { get; set; }

        [JsonProperty("uploadLimited")]
        public bool UploadLimited { get; set; }

        [JsonProperty("uploadRatio")]
        public double uploadRatio { get; set; }

        [JsonProperty("wanted")]
        public bool[] Wanted { get; set; }

        [JsonProperty("webseeds")]
        public string[] Webseeds { get; set; }

        [JsonProperty("webseedsSendingToUs")]
        public int WebseedsSendingToUs { get; set; }
    }

    public class TransmissionTorrentFiles
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted{ get; set; }

        [JsonProperty("length")]
        public double Length{ get; set; }

        [JsonProperty("name")]
        public string Name{ get; set; }
    }

    public class TransmissionTorrentFileStats
    {
        [JsonProperty("bytesCompleted")]
        public double BytesCompleted{ get; set; }

        [JsonProperty("wanted")]
        public bool Wanted{ get; set; }

        [JsonProperty("priority")]
        public int Priority{ get; set; }
    }

    public class TransmissionTorrentPeers
    {
        [JsonProperty("address")]
        public string Address{ get; set; }

        [JsonProperty("clientName")]
        public string ClientName{ get; set; }

        [JsonProperty("clientIsChoked")]
        public bool ClientIsChoked{ get; set; }

        [JsonProperty("clientIsInterested")]
        public bool ClientIsInterested{ get; set; }

        [JsonProperty("flagStr")]
        public string FlagStr{ get; set; }

        [JsonProperty("isDownloadingFrom")]
        public bool IsDownloadingFrom{ get; set; }

        [JsonProperty("isEncrypted")]
        public bool IsEncrypted{ get; set; }

        [JsonProperty("isUploadingTo")]
        public bool IsUploadingTo{ get; set; }

        [JsonProperty("isUTP")]
        public bool IsUTP{ get; set; }

        [JsonProperty("peerIsChoked")]
        public bool PeerIsChoked{ get; set; }

        [JsonProperty("peerIsInterested")]
        public bool PeerIsInterested{ get; set; }

        [JsonProperty("port")]
        public int Port{ get; set; }

        [JsonProperty("progress")]
        public double Progress{ get; set; }

        [JsonProperty("rateToClient")]
        public int RateToClient{ get; set; }

        [JsonProperty("rateToPeer")]
        public int RateToPeer{ get; set; }
    }

    public class TransmissionTorrentPeersFrom
    {
        [JsonProperty("fromDht")]
        public int FromDHT{ get; set; }

        [JsonProperty("fromIncoming")]
        public int FromIncoming{ get; set; }

        [JsonProperty("fromLpd")]
        public int FromLPD{ get; set; }

        [JsonProperty("fromLtep")]
        public int FromLTEP{ get; set; }

        [JsonProperty("fromPex")]
        public int FromPEX{ get; set; }

        [JsonProperty("fromTracker")]
        public int FromTracker{ get; set; }
    }

    public class TransmissionTorrentTrackers
    {
        [JsonProperty("announce")]
        public string announce{ get; set; }

        [JsonProperty("id")]
        public int ID{ get; set; }

        [JsonProperty("scrape")]
        public string Scrape{ get; set; }

        [JsonProperty("tier")]
        public int Tier{ get; set; }
    }

    public class TransmissionTorrentTrackerStats
    {

        [JsonProperty("announce")]
        public string announce{ get; set; }

        [JsonProperty("announceState")]
        public int AnnounceState{ get; set; }

        [JsonProperty("downloadCount")]
        public int DownloadCount{ get; set; }

        [JsonProperty("hasAnnounced")]
        public bool HasAnnounced{ get; set; }

        [JsonProperty("hasScraped")]
        public bool HasScraped{ get; set; }

        [JsonProperty("host")]
        public string Host{ get; set; }

        [JsonProperty("isBackup")]
        public bool IsBackup{ get; set; }

        [JsonProperty("lastAnnouncePeerCount")]
        public int LastAnnouncePeerCount{ get; set; }

        [JsonProperty("id")]
        public int ID{ get; set; }

        [JsonProperty("lastAnnounceResult")]
        public string LastAnnounceResult{ get; set; }

        [JsonProperty("lastAnnounceSucceeded")]
        public bool LastAnnounceSucceeded{ get; set; }

        [JsonProperty("lastAnnounceStartTime")]
        public int LastAnnounceStartTime{ get; set; }

        [JsonProperty("lastScrapeResult")]
        public string LastScrapeResult{ get; set; }

        [JsonProperty("lastAnnounceTimedOut")]
        public bool LastAnnounceTimedOut{ get; set; }

        [JsonProperty("lastAnnounceTime")]
        public int LastAnnounceTime{ get; set; }

        [JsonProperty("lastScrapeSucceeded")]
        public bool LastScrapeSucceeded{ get; set; }

        [JsonProperty("lastScrapeStartTime")]
        public int LastScrapeStartTime{ get; set; }

        [JsonProperty("lastScrapeTimedOut")]
        public bool LastScrapeTimedOut{ get; set; }

        [JsonProperty("lastScrapeTime")]
        public int LastScrapeTime{ get; set; }

        [JsonProperty("scrape")]
        public string Scrape{ get; set; }

        [JsonProperty("tier")]
        public int Tier{ get; set; }

        [JsonProperty("leecherCount")]
        public int LeecherCount{ get; set; }

        [JsonProperty("nextAnnounceTime")]
        public int NextAnnounceTime{ get; set; }

        [JsonProperty("nextScrapeTime")]
        public int NextScrapeTime{ get; set; }

        [JsonProperty("scrapeState")]
        public int ScrapeState{ get; set; }

        [JsonProperty("seederCount")]
        public int SeederCount{ get; set; }
    }

    //TODO: Separate "remove" and "active" torrents in "torrentsGet"
    /// <summary>
    /// Contains arrays of torrents and removed torrents
    /// </summary>
    public class TransmissionTorrents
    {
        /// <summary>
        /// Array of torrents
        /// </summary>
        [JsonProperty("torrents")]
        public TorrentInfo[] Torrents{ get; set; }

        /// <summary>
        /// Array of torrent-id numbers of recently-removed torrents
        /// </summary>
        [JsonProperty("removed")]
        public TorrentInfo[] Removed{ get; set; }
    }
}
