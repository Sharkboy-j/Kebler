using System.Linq;
using AutoMapper;

namespace Kebler.Mapper
{
    public class MapStatistic
    {
        public static IMapper Mapper { get; }

        static MapStatistic()
        {
            var mapperConfig = ConfigureMap();
            //mapperConfig.AssertConfigurationIsValid();

            Mapper = mapperConfig.CreateMapper();
        }

        private static MapperConfiguration ConfigureMap()
        {
            return new MapperConfiguration(configure: cfg =>
            {
                cfg.CreateMap<Transmission.Models.Entities.SessionInfo, UI.Models.Statistic>();
                cfg.CreateMap<Transmission.Models.Entities.Statistic, UI.Models.Statistic>().ConstructUsing(x =>
                    new UI.Models.Statistic()
                    {
                        UploadedBytes = x.CumulativeStats.UploadedBytes,
                        DownloadedBytes = x.CumulativeStats.DownloadedBytes,
                        Uptime = x.CumulativeStats.SecondsActive,
                        FilesAdded = x.CumulativeStats.FilesAdded,
                    });

                cfg.CreateMap<Kebler.QBittorrent.GlobalTransferInfo, UI.Models.Statistic>().ConstructUsing(x =>
                    new UI.Models.Statistic()
                    {
                        UploadedBytes = x.UploadedData ?? default,
                        DownloadedBytes = x.DownloadedData ?? default,
                        Uptime = default,
                        FilesAdded = default,
                    });
                cfg.CreateMap<Kebler.QBittorrent.GlobalTransferInfo, Kebler.Domain.Interfaces.Torrents.IStatistic>().ConstructUsing(x =>
                    new UI.Models.Statistic()
                    {
                        UploadedBytes = x.UploadedData ?? default,
                        DownloadedBytes = x.DownloadedData ?? default,
                        Uptime = default,
                        FilesAdded = default,
                    });

                //cfg.CreateMap<Transmission.Models.Entities.SessionInfo, UI.Models.Statistic>().ConstructUsing(x => new UI.Models.Statistic()
                //{
                //    AlternativeSpeedDown = x.AlternativeSpeedDown,
                //    AlternativeSpeedEnabled = x.AlternativeSpeedEnabled,
                //    AlternativeSpeedTimeBegin = x.AlternativeSpeedTimeBegin,
                //    AlternativeSpeedTimeEnabled = x.AlternativeSpeedTimeEnabled,
                //    AlternativeSpeedTimeEnd = x.AlternativeSpeedTimeEnd,
                //    AlternativeSpeedTimeDay = x.AlternativeSpeedTimeDay,
                //    AlternativeSpeedUp = x.AlternativeSpeedUp,
                //    BlocklistURL = x.BlocklistURL,
                //    BlocklistEnabled = x.BlocklistEnabled,
                //    CacheSizeMB = x.CacheSizeMB,
                //    DownloadDirectory = x.DownloadDirectory,
                //    DownloadQueueSize = x.DownloadQueueSize,
                //    DownloadQueueEnabled = x.DownloadQueueEnabled,
                //    DHTEnabled = x.DHTEnabled,
                //    Encryption = x.Encryption,
                //    IdleSeedingLimit = x.IdleSeedingLimit,
                //    IdleSeedingLimitEnabled = x.IdleSeedingLimitEnabled,
                //    IncompleteDirectory = x.IncompleteDirectory,
                //    IncompleteDirectoryEnabled = x.IncompleteDirectoryEnabled,
                //    LPDEnabled = x.LPDEnabled,
                //    PeerLimitGlobal = x.PeerLimitGlobal,
                //    PeerLimitPerTorrent = x.PeerLimitPerTorrent,
                //    PexEnabled = x.PexEnabled,
                //    PeerPort = x.PeerPort,
                //    PeerPortRandomOnStart = x.PeerPortRandomOnStart,
                //    PortForwardingEnabled = x.PortForwardingEnabled,
                //    QueueStalledEnabled = x.QueueStalledEnabled,
                //    QueueStalledMinutes = x.QueueStalledMinutes,
                //    RenamePartialFiles = x.RenamePartialFiles,
                //    ScriptTorrentDoneFilename = x.ScriptTorrentDoneFilename,
                //    ScriptTorrentDoneEnabled = x.ScriptTorrentDoneEnabled,
                //    SeedRatioLimit = x.SeedRatioLimit,
                //    SeedRatioLimited = x.SeedRatioLimited,
                //    SeedQueueSize = x.SeedQueueSize,
                //    SeedQueueEnabled = x.SeedQueueEnabled,
                //    SpeedLimitDown = x.SpeedLimitDown,
                //    SpeedLimitDownEnabled = x.SpeedLimitDownEnabled,
                //    SpeedLimitUp = x.SpeedLimitUp,
                //    SpeedLimitUpEnabled = x.SpeedLimitUpEnabled,
                //    StartAddedTorrents = x.StartAddedTorrents,
                //    TrashOriginalTorrentFiles = x.TrashOriginalTorrentFiles,
                //    UtpEnabled = x.UtpEnabled,
                //    BlocklistSize = x.BlocklistSize,
                //    ConfigDirectory = x.ConfigDirectory,
                //    RpcVersion = x.RpcVersion,
                //    RpcVersionMinimum = x.RpcVersionMinimum,
                //    Version = x.Version
                //});

                //cfg.AddGlobalIgnore(nameof(UI.Models.Statistic.IsNotifying));
            });
        }
    }


    public static class MapStaticExt
    {
        public static TResult MergeInto<TResult>(this IMapper mapper, object item1, object item2)
        {
            return mapper.Map(item2, mapper.Map<TResult>(item1));
        }

        public static TResult MergeInto<TResult>(this IMapper mapper, params object[] objects)
        {
            var res = mapper.Map<TResult>(objects.First());
            return objects.Skip(1).Aggregate(res, (r, obj) => mapper.Map(obj, r));
        }
    }
}
