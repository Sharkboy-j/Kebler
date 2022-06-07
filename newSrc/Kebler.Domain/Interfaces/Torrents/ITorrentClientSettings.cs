namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentClientSettings
    {
        long AlternativeSpeedDown { get; set; }
        bool? AlternativeSpeedEnabled { get; set; }
        int? AlternativeSpeedTimeBegin { get; set; }
        int? AlternativeSpeedTimeDay { get; set; }
        bool? AlternativeSpeedTimeEnabled { get; set; }
        int? AlternativeSpeedTimeEnd { get; set; }
        long AlternativeSpeedUp { get; set; }
        bool? BlocklistEnabled { get; set; }
        string BlocklistURL { get; set; }
        int? CacheSizeMB { get; set; }
        bool? DHTEnabled { get; set; }
        string DownloadDirectory { get; set; }
        bool? DownloadQueueEnabled { get; set; }
        int? DownloadQueueSize { get; set; }
        string Encryption { get; set; }
        int? IdleSeedingLimit { get; set; }
        bool? IdleSeedingLimitEnabled { get; set; }
        string IncompleteDirectory { get; set; }
        bool? IncompleteDirectoryEnabled { get; set; }
        bool? LPDEnabled { get; set; }
        int? PeerLimitGlobal { get; set; }
        int? PeerLimitPerTorrent { get; set; }
        int? PeerPort { get; set; }
        bool? PeerPortRandomOnStart { get; set; }
        bool? PexEnabled { get; set; }
        bool? PortForwardingEnabled { get; set; }
        bool? QueueStalledEnabled { get; set; }
        int? QueueStalledMinutes { get; set; }
        bool? RenamePartialFiles { get; set; }
        bool? ScriptTorrentDoneEnabled { get; set; }
        string ScriptTorrentDoneFilename { get; set; }
        bool? SeedQueueEnabled { get; set; }
        int? SeedQueueSize { get; set; }
        double? SeedRatioLimit { get; set; }
        bool? SeedRatioLimited { get; set; }
        int? SpeedLimitDown { get; set; }
        bool? SpeedLimitDownEnabled { get; set; }
        int? SpeedLimitUp { get; set; }
        bool? SpeedLimitUpEnabled { get; set; }
        bool? StartAddedTorrents { get; set; }
        bool? TrashOriginalTorrentFiles { get; set; }
        bool? UtpEnabled { get; set; }
    }
}