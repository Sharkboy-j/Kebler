using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Interfaces.Torrents;
using Kebler.Mapper;
using Kebler.QBittorrent;

namespace Kebler.Workers
{
    public class QBitTorrentWorker : IWorker
    {
        private readonly IQBittorrentClient _client;
        private readonly ILogger _logger;
        private readonly IServer _server;
        private readonly string _password;


        public static IWorker CreateInstance(in IServer server, in ILogger logger, in string password)
        {
            var worker = new QBitTorrentWorker(in server, in logger, in password);
            return worker;
        }

        public QBitTorrentWorker(in IServer server, in ILogger logger, in string password)
        {
            _logger = logger;
            _server = server;
            _password = password;
            _client = new QBittorrentClient(new Uri(server.FullUriPath));
        }


        public Task<(bool, Exception)> CheckConnectionAsync(int timeOut = 50000, bool disconnectAfter = false)
        {
            return CheckConnectionAsync(TimeSpan.FromMilliseconds(timeOut), disconnectAfter);
        }

        public async Task<(bool, Exception)> CheckConnectionAsync(TimeSpan timeOut, bool disconnectAfter = false)
        {
            try
            {
                var tokenSource = new CancellationTokenSource(timeOut);
                if (_server.AuthEnabled)
                    await _client.LoginAsync(_server.UserName, _password, tokenSource.Token);

                _ = await _client.GetQBittorrentVersionAsync(tokenSource.Token);
                var result = await _client.GetGlobalTransferInfoAsync(tokenSource.Token);

                if (disconnectAfter)
                    await _client.LogoutAsync(tokenSource.Token);

                return result != null
                    ? result.ConnectionStatus == ConnectionStatus.Connected
                        ? (true, null)
                        : (false, new Exception($"{nameof(result.ConnectionStatus)} => {result.ConnectionStatus}"))
                    : (false, new Exception("Unknown exception"));
            }
            catch (TaskCanceledException)
            {
                return (false, new Exception("Time out reached"));
            }
            catch (QBittorrentClientRequestException ex)
            {
                return (false, ex);
            }
            catch (Exception e)
            {
                return (false, e);
            }
        }

        public async Task<(IStatistic, Exception)> GetSessionStatisticAsync(CancellationToken token)
        {
            try
            {
                var response = await _client.GetGlobalTransferInfoAsync(token);
                var version = await _client.GetQBittorrentVersionAsync(token);

                var result = MapStatistic.Mapper.Map<IStatistic>(response);

                result.Version = version.ToString();

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<(IEnumerable<ITorrent>, Exception)> TorrentsGetAsync(CancellationToken token)
        {
            try
            {
                var response = await _client.GetTorrentListAsync(new TorrentListQuery(), token);

                var result = TorrentMapper.Mapper.Map<IEnumerable<ITorrent>>(response);

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<(ITorrentClientSettings, Exception)> GetTorrentClientSettingsAsync(CancellationToken token)
        {
            try
            {
                var response = await _client.GetPreferencesAsync(token);

                var result = SettingsMapper.Mapper.Map<ITorrentClientSettings>(response);

                return (result, null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }
    }
}