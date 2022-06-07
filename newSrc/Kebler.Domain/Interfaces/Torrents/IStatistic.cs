using System.ComponentModel;

namespace Kebler.Domain.Interfaces.Torrents
{
    public interface IStatistic : INotifyPropertyChanged
    {
        long ActiveTorrentCount { get; set; }
        long DownloadSpeed { get; set; }
        long UploadSpeed { get; set; }
        long PausedTorrentCount { get; set; }
        long TorrentCount { get; set; }
        long UploadedBytes { get; set; }
        long DownloadedBytes { get; set; }
        long Uptime { get; set; }
        long FilesAdded { get; set; }
        long AlternativeSpeedDown { get; set; }
        bool AlternativeSpeedEnabled { get; set; }
        long AlternativeSpeedTimeBegin { get; set; }
        bool AlternativeSpeedTimeEnabled { get; set; }
        long AlternativeSpeedTimeEnd { get; set; }
        long AlternativeSpeedTimeDay { get; set; }
        long AlternativeSpeedUp { get; set; }
        string BlocklistURL { get; set; }
        bool BlocklistEnabled { get; set; }
        long CacheSizeMB { get; set; }
        string DownloadDirectory { get; set; }
        long DownloadQueueSize { get; set; }
        bool DownloadQueueEnabled { get; set; }
        bool DHTEnabled { get; set; }
        string Encryption { get; set; }
        long IdleSeedingLimit { get; set; }
        bool IdleSeedingLimitEnabled { get; set; }
        string IncompleteDirectory { get; set; }
        bool IncompleteDirectoryEnabled { get; set; }
        bool LPDEnabled { get; set; }
        long PeerLimitGlobal { get; set; }
        long PeerLimitPerTorrent { get; set; }
        bool PexEnabled { get; set; }
        long PeerPort { get; set; }
        bool PeerPortRandomOnStart { get; set; }
        bool PortForwardingEnabled { get; set; }
        bool QueueStalledEnabled { get; set; }
        long QueueStalledMinutes { get; set; }
        bool RenamePartialFiles { get; set; }
        string ScriptTorrentDoneFilename { get; set; }
        bool ScriptTorrentDoneEnabled { get; set; }
        double SeedRatioLimit { get; set; }
        bool SeedRatioLimited { get; set; }
        long SeedQueueSize { get; set; }
        bool SeedQueueEnabled { get; set; }
        long SpeedLimitDown { get; set; }
        bool SpeedLimitDownEnabled { get; set; }
        long SpeedLimitUp { get; set; }
        bool SpeedLimitUpEnabled { get; set; }
        bool StartAddedTorrents { get; set; }
        bool TrashOriginalTorrentFiles { get; set; }
        bool UtpEnabled { get; set; }
        long BlocklistSize { get; set; }
        string ConfigDirectory { get; set; }
        long RpcVersion { get; set; }
        long RpcVersionMinimum { get; set; }
        string Version { get; set; }
    }
}