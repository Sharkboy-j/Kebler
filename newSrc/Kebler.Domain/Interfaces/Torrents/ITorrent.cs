using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrent : INotifyPropertyChanged
    {
        uint Id { get; }
        long AddedDate { get; set; }
        int BandwidthPriority { get; set; }
        string Comment { get; set; }
        int CorruptEver { get; set; }
        string Creator { get; set; }
        int DateCreated { get; set; }
        long DesiredAvailable { get; set; }
        long DoneDate { get; set; }
        string DownloadDir { get; set; }
        long DownloadedEver { get; set; }
        long DownloadLimit { get; set; }
        bool DownloadLimited { get; set; }
        int Error { get; set; }
        string ErrorString { get; set; }
        int ETA { get; set; }
        int ETAIdle { get; set; }
        IEnumerable<ITorrentFiles> Files { get; set; }
        IEnumerable<ITorrentFileStats> FileStats { get; set; }
        string HashString { get; set; }
        int HaveUnchecked { get; set; }
        long HaveValid { get; set; }
        bool HonorsSessionLimits { get; set; }
        bool IsFinished { get; set; }
        bool IsPrivate { get; set; }
        bool IsStalled { get; set; }
        long LeftUntilDone { get; set; }
        string MagnetLink { get; set; }
        int ManualAnnounceTime { get; set; }
        int MaxConnectedPeers { get; set; }
        double MetadataPercentComplete { get; set; }
        string Name { get; set; }
        int PeerLimit { get; set; }
        IEnumerable<ITorrentPeers> Peers { get; set; }
        int PeersConnected { get; set; }
        ITorrentPeersFrom PeersFrom { get; set; }
        int PeersSendingToUs { get; set; }
        double PercentDone { get; set; }
        string Pieces { get; set; }
        int PieceCount { get; set; }
        int PieceSize { get; set; }
        IEnumerable<int> Priorities { get; set; }
        int QueuePosition { get; set; }
        int RateDownload { get; set; }
        int RateUpload { get; set; }
        double RecheckProgress { get; set; }
        int SecondsDownloading { get; set; }
        int SecondsSeeding { get; set; }
        int SeedIdleLimit { get; set; }
        int SeedIdleMode { get; set; }
        double SeedRatioLimit { get; set; }
        int SeedRatioMode { get; set; }
        long SizeWhenDone { get; set; }
        int StartDate { get; set; }

        /// <summary>
        /// <para>0: 'stopped'</para> 
        /// <para>1: 'check pending'</para> 
        /// <para>2: 'checking'</para> 
        /// <para>3: 'download pending'</para> 
        /// <para>4: 'downloading'</para> 
        /// <para>5: 'seed pending'</para> 
        /// <para>6: 'seeding' </para> 
        /// </summary>
        ///
        int Status { get; set; }

        IEnumerable<ITorrentTrackers> Trackers { get; set; }
        IEnumerable<ITorrentTrackerStats> TrackerStats { get; set; }
        long TotalSize { get; set; }
        string TorrentFile { get; set; }
        long UploadedEver { get; set; }
        long UploadLimit { get; set; }
        bool UploadLimited { get; set; }
        double UploadRatio { get; set; }
        IEnumerable<bool> Wanted { get; set; }
        IEnumerable<string> WebSeeds { get; set; }
        int WebseedsSendingToUs { get; set; }

        public void UpdateData(ITorrent inf);
    }
}
