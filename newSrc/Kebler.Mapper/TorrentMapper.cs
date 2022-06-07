using System.Collections.Generic;
using AutoMapper;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.UI.Models;

namespace Kebler.Mapper
{
    public class TorrentMapper
    {
        private static IMapper _mapper;

        public static IMapper Mapper
        {
            get
            {
                return _mapper ??= CreateMapper();
            }
        }

        public static IMapper CreateMapper()
        {
            var mapperConfig = ConfigureMap();
            mapperConfig.AssertConfigurationIsValid();

            return mapperConfig.CreateMapper();
        }

        private static MapperConfiguration ConfigureMap()
        {
            return new MapperConfiguration(configure: cfg =>
            {
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFiles, UI.Models.TorrentFiles>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFiles, ITorrentFiles>()
                    .ConstructUsing(x => new TorrentFiles());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentFileStats, UI.Models.TorrentFileStats>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFileStats, ITorrentFileStats>().ConstructUsing(x => new TorrentFileStats());
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeers, UI.Models.TorrentPeers>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeers, ITorrentPeers>().ConstructUsing(x => new TorrentPeers());
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackers, UI.Models.TorrentTrackers>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackers, ITorrentTrackers>().ConstructUsing(x => new TorrentTrackers());
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackerStats, UI.Models.TorrentTrackerStats>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackerStats, ITorrentTrackerStats>().ConstructUsing(x => new TorrentTrackerStats());
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeersFrom, UI.Models.TorrentPeersFrom>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeersFrom, ITorrentPeersFrom>().ConstructUsing(x => new TorrentPeersFrom());


                cfg.CreateMap<Transmission.Models.TorrentInfo, UI.Models.Torrent>()
                    .ConstructUsing(x => new UI.Models.Torrent(x.Id, x.Name)
                    {
                        RateDownload = x.RateDownload,
                        RateUpload = x.RateUpload,
                        Files = Mapper.Map<List<UI.Models.TorrentFiles>>(x.Files),
                        FileStats = Mapper.Map<List<UI.Models.TorrentFileStats>>(x.FileStats),
                        Peers = Mapper.Map<List<UI.Models.TorrentPeers>>(x.Peers),
                        Trackers = Mapper.Map<List<UI.Models.TorrentTrackers>>(x.Trackers),
                        TrackerStats = Mapper.Map<List<UI.Models.TorrentTrackerStats>>(x.TrackerStats),
                    });
                cfg.AddGlobalIgnore(nameof(UI.Models.Torrent.IsNotifying));
            });
        }
    }
}