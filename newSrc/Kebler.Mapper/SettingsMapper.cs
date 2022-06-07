using AutoMapper;

namespace Kebler.Mapper
{
    public class SettingsMapper
    {
        private static IMapper _mapper;

        public static IMapper Mapper => _mapper ??= CreateMapper();

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
                cfg.CreateMap<Transmission.Models.Arguments.SessionSettings, UI.Models.TorrentClientSettings>();

                cfg.CreateMap<Kebler.QBittorrent.Preferences, Domain.Interfaces.Torrents.ITorrentClientSettings>().ConstructUsing(x =>
                    new UI.Models.TorrentClientSettings()
                    {
                        SpeedLimitUp = x.UploadLimit,
                        AlternativeSpeedUp = x.AlternativeUploadLimit ?? default,
                    });

                cfg.AddGlobalIgnore(nameof(UI.Models.Torrent.IsNotifying));
            });
        }
    }
}