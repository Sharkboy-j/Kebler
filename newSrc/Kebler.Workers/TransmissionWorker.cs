using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Mapper;

namespace Kebler.Workers
{
    public class TransmissionWorker : IWorker
    {
        private readonly Transmission.TransmissionClient _client;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly ILogger _logger;

        public static IWorker CreateInstance(in IServer server, in ILogger logger, in string password = null)
        {
            var worker = new TransmissionWorker(in server, in logger, in password);
            return worker;
        }

        public TransmissionWorker(in IServer server, in ILogger logger, in string password)
        {
            _logger = logger;
            _cancelTokenSource = new CancellationTokenSource();
            _client = new Transmission.TransmissionClient(server.FullUriPath, null, server.UserName, in password);
        }

        public async Task<(bool, Exception)> CheckConnectionAsync(int timeOut = default)
        {
            try
            {
                var tokenSource = timeOut == default ? new CancellationTokenSource() : new CancellationTokenSource(timeOut);

                _logger.Trace($"{nameof(CheckConnectionAsync)} Start");

                var info = await _client.GetSessionInformationAsync(tokenSource.Token);

                if (info.Response.Success)
                {
                    _logger.Trace($"{nameof(CheckConnectionAsync)} Success");
                    return (true, null);
                }
                else
                {
                    if (info.Response?.WebException != null)
                    {
                        return (false, info.Response?.WebException);
                    }
                    else if (info.Response.CustomException != null)
                    {
                        return (false, info.Response?.CustomException);
                    }
                    else
                    {
                        return (false, new Exception("Unknown"));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Trace($"{nameof(CheckConnectionAsync)} failed");
                _logger.Error(ex);
                //Crashes.TrackError(ex);

                return (false, ex);
            }
            finally
            {
                _logger.Trace($"{nameof(CheckConnectionAsync)} ended");
            }
        }

        public async Task<IStatistic> GetSessionStatisticAsync(CancellationToken token)
        {
            var stats = await _client.GetSessionStatisticAsync(token);
            var session = await _client.GetSessionInformationAsync(token);

            var fromstats = MapStatistic.Mapper.Map<Transmission.Models.Entities.Statistic, UI.Models.Statistic>(stats.Value);
            var fromstatsAndSession = MapStatistic.Mapper.Map(session.Value, fromstats);

            token.ThrowIfCancellationRequested();

            return fromstatsAndSession;
        }

        public async Task<IEnumerable<ITorrent>> TorrentsGetAsync(CancellationToken token)
        {
            var result = await _client.TorrentGetAsync(Transmission.Models.TorrentFields.WORK, token);

            var staticDa = TorrentMapper.Mapper.Map<List<UI.Models.Torrent>>(result.Value.Torrents);

            token.ThrowIfCancellationRequested();

            return staticDa;
        }

        public async Task<ITorrentClientSettings> GetTorrentClientSettingsAsync(CancellationToken token)
        {
            var result = await _client.GetSessionSettingsAsync(token);

            var settings = SettingsMapper.Mapper.Map<UI.Models.TorrentClientSettings>(result.Value);

            token.ThrowIfCancellationRequested();

            return settings;
        }
    }
}