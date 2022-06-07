using System;
using System.Collections.Generic;
using AutoMapper;

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
            //mapperConfig.AssertConfigurationIsValid();

            return mapperConfig.CreateMapper();
        }

        private static MapperConfiguration ConfigureMap()
        {
            return new MapperConfiguration(configure: cfg =>
            {
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFiles, UI.Models.TorrentFiles>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFiles, Domain.Interfaces.Torrents.ITorrentFiles>()
                    .ConstructUsing(x => new UI.Models.TorrentFiles());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentFileStats, UI.Models.TorrentFileStats>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentFileStats, Domain.Interfaces.Torrents.ITorrentFileStats>()
                    .ConstructUsing(x => new UI.Models.TorrentFileStats());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeers, UI.Models.TorrentPeers>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeers, Domain.Interfaces.Torrents.ITorrentPeers>()
                    .ConstructUsing(x => new UI.Models.TorrentPeers());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackers, UI.Models.TorrentTrackers>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackers, Domain.Interfaces.Torrents.ITorrentTrackers>()
                    .ConstructUsing(x => new UI.Models.TorrentTrackers());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackerStats, UI.Models.TorrentTrackerStats>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentTrackerStats, Domain.Interfaces.Torrents.ITorrentTrackerStats>()
                    .ConstructUsing(x => new UI.Models.TorrentTrackerStats());

                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeersFrom, UI.Models.TorrentPeersFrom>();
                cfg.CreateMap<Transmission.Models.TransmissionTorrentPeersFrom, Domain.Interfaces.Torrents.ITorrentPeersFrom>()
                    .ConstructUsing(x => new UI.Models.TorrentPeersFrom());

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




                cfg.CreateMap<QBittorrent.TorrentInfo, Domain.Interfaces.Torrents.ITorrent>()
                    .ConstructUsing(x => new UI.Models.Torrent(default, x.Name)
                    {
                        RateDownload = x.DownloadSpeed,
                        RateUpload = x.UploadSpeed,
                        HashString = x.Hash,
                        Status = GetState(x.State)
                        //Files = Mapper.Map<List<UI.Models.TorrentFiles>>(x),
                        //FileStats = Mapper.Map<List<UI.Models.TorrentFileStats>>(x.FileStats),
                        //Peers = Mapper.Map<List<UI.Models.TorrentPeers>>(x),
                        //Trackers = Mapper.Map<List<UI.Models.TorrentTrackers>>(x),
                        //TrackerStats = Mapper.Map<List<UI.Models.TorrentTrackerStats>>(x),
                    });

                //cfg.CreateMap<QBittorrent.TorrentInfo, UI.Models.Torrent>()
                //    .ConstructUsing(x => new UI.Models.Torrent(default, x.Name)
                //    {
                //        RateDownload = x.DownloadSpeed,
                //        RateUpload = x.UploadSpeed,
                //        HashString = x.Hash,
                //        //Files = Mapper.Map<List<UI.Models.TorrentFiles>>(x),
                //        //FileStats = Mapper.Map<List<UI.Models.TorrentFileStats>>(x.FileStats),
                //        //Peers = Mapper.Map<List<UI.Models.TorrentPeers>>(x),
                //        //Trackers = Mapper.Map<List<UI.Models.TorrentTrackers>>(x),
                //        //TrackerStats = Mapper.Map<List<UI.Models.TorrentTrackerStats>>(x),
                //    });



                cfg.AddGlobalIgnore(nameof(UI.Models.Torrent.IsNotifying));
            });
        }

        private static int GetState(QBittorrent.TorrentState state)
        {
            /// <para>0: 'stopped'</para> 
            /// <para>1: 'check pending'</para> 
            /// <para>2: 'checking'</para> 
            /// <para>3: 'download pending'</para> 
            /// <para>4: 'downloading'</para> 
            /// <para>5: 'seed pending'</para> 
            /// <para>6: 'seeding' </para> 
            switch (state)
            {
                case QBittorrent.TorrentState.Unknown:
                case QBittorrent.TorrentState.Error:
                case QBittorrent.TorrentState.MissingFiles:
                case QBittorrent.TorrentState.PausedUpload:
                case QBittorrent.TorrentState.PausedDownload:
                case QBittorrent.TorrentState.QueuedUpload:
                case QBittorrent.TorrentState.QueuedDownload:
                case QBittorrent.TorrentState.StalledUpload:
                case QBittorrent.TorrentState.StalledDownload:
                case QBittorrent.TorrentState.FetchingMetadata:
                case QBittorrent.TorrentState.Allocating:
                case QBittorrent.TorrentState.Moving:
                    return 0;
                case QBittorrent.TorrentState.ForcedUpload:
                case QBittorrent.TorrentState.Uploading:
                    return 6;
                case QBittorrent.TorrentState.CheckingUpload:
                    return 2;
                case QBittorrent.TorrentState.CheckingDownload:
                    return 2;
                case QBittorrent.TorrentState.ForcedDownload:
                case QBittorrent.TorrentState.Downloading:
                    return 4;
                case QBittorrent.TorrentState.QueuedForChecking:
                case QBittorrent.TorrentState.CheckingResumeData:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}