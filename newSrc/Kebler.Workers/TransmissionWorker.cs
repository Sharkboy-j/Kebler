using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Domain.Models;
using Kebler.Mapper;
using Kebler.Transmission.Models;
using Kebler.Transmission.Models.Arguments;

namespace Kebler.Workers
{
    public class TransmissionWorker : IWorker
    {
        private readonly Transmission.TransmissionClient _client;
        private readonly ILogger _logger;

        public static IWorker CreateInstance(in IServer server, in ILogger logger, in string password = null)
        {
            var worker = new TransmissionWorker(in server, in logger, in password);
            return worker;
        }

        public TransmissionWorker(in IServer server, in ILogger logger, in string password)
        {
            _logger = logger;
            _client = new Transmission.TransmissionClient(server.FullUriPath, null, server.UserName, in password);
        }

        private void CheckResponce(ITransmissionResponse response, out Exception exception)
        {
            exception = null;

            if (response?.WebException != null)
            {
                exception = response?.WebException;
            }
            else if (response?.CustomException != null)
            {
                exception = response?.WebException;
            }

        }

        public Task<(bool, Exception)> CheckConnectionAsync(int timeOut = default, bool disconnectAfter = false)
        {
            return CheckConnectionAsync(TimeSpan.FromSeconds(timeOut));
        }

        public async Task<(bool, Exception)> CheckConnectionAsync(TimeSpan timeOut = default, bool disconnectAfter = false)
        {
            try
            {
                var tokenSource = timeOut == default ? new CancellationTokenSource() : new CancellationTokenSource(timeOut);

                _logger.Trace($"{nameof(CheckConnectionAsync)} Start");

                var result = await _client.GetSessionInformationAsync(tokenSource.Token);

                CheckResponce(result.Response, out var exception);
                if (exception != null)
                {
                    _logger.Trace($"{nameof(CheckConnectionAsync)} failed");
                    return (false, exception);
                }
                _logger.Trace($"{nameof(CheckConnectionAsync)} Success");
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.Trace($"{nameof(CheckConnectionAsync)} failed");
                _logger.Error(ex);

                return (false, ex);
            }
            finally
            {
                _logger.Trace($"{nameof(CheckConnectionAsync)} ended");
            }
        }


        public async Task<(IStatistic, Exception)> GetSessionStatisticAsync(CancellationToken token)
        {
            var stats = await _client.GetSessionStatisticAsync(token);
            var session = await _client.GetSessionInformationAsync(token);

            CheckResponce(stats.Response, out var exception);
            if (exception != null)
            {
                return (null, exception);
            }

            CheckResponce(stats.Response, out exception);
            if (exception != null)
            {
                return (null, exception);
            }

            var fromstats = MapStatistic.Mapper.Map<Transmission.Models.Entities.Statistic, UI.Models.Statistic>(stats.Value);
            var fromstatsAndSession = MapStatistic.Mapper.Map(session.Value, fromstats);

            token.ThrowIfCancellationRequested();

            return (fromstatsAndSession, null);
        }

        public async Task<(IEnumerable<ITorrent>, Exception)> TorrentsGetAsync(CancellationToken token)
        {
            var result = await _client.TorrentGetAsync(TorrentFields.Work, token);

            CheckResponce(result.Response, out var exception);
            if (exception != null)
            {
                return (null, exception);
            }

            var staticDa = TorrentMapper.Mapper.Map<List<UI.Models.Torrent>>(result.Value.Torrents);

            token.ThrowIfCancellationRequested();

            return (staticDa, null);
        }

        public async Task<(ITorrentClientSettings, Exception)> GetTorrentClientSettingsAsync(CancellationToken token)
        {
            var result = await _client.GetSessionSettingsAsync(token);

            CheckResponce(result.Response, out var exception);
            if (exception != null)
            {
                return (null, exception);
            }

            var settings = SettingsMapper.Mapper.Map<UI.Models.TorrentClientSettings>(result.Value);

            token.ThrowIfCancellationRequested();

            return (settings, null);
        }

        public async Task<IEnumerable<AddTorrentResult>> TorrentsAddAsync(IEnumerable<INewTorrent> torrents, CancellationToken token)
        {
            var results = new List<AddTorrentResult>();

            foreach (var torrent in torrents)
            {
                try
                {
                    var newTorrent = TorrentMapper.Mapper.Map<Transmission.Models.Arguments.NewTorrent>(torrent);

                    var response = await _client.TorrentAddAsync(newTorrent, token);

                    switch (response.Value.Status)
                    {
                        case Enums.AddTorrentStatus.Added:
                            results.Add(new AddTorrentResult(true, null, torrent));
                            break;
                        case Enums.AddTorrentStatus.Duplicate:
                            results.Add(new AddTorrentResult(true, new Exception("Duplicated torrent"), torrent));
                            break;
                        case Enums.AddTorrentStatus.UnknownError:
                        case Enums.AddTorrentStatus.ResponseNull:
                            throw new Exception("Unknown error");
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new AddTorrentResult(false, ex, torrent));
                }
            }

            return results;
        }
    }
}