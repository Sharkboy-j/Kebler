using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces;
using Kebler.Transmission;

namespace Kebler.Workers
{
    public class TransmissionWorker : IRemoteTorrentClient
    {
        private readonly TransmissionClient _client;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly ILogger _logger;

        public static IRemoteTorrentClient CreateInstance(IServer server, ILogger logger)
        {
            var worker = new TransmissionWorker(server, logger);
            return worker;
        }

        public TransmissionWorker(IServer server, ILogger logger)
        {
            _logger = logger;
            _cancelTokenSource = new CancellationTokenSource();
            _client = new TransmissionClient(server.FullUriPath, null, server.UserName, server.Password);
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
                    else if(info.Response.CustomException!=null)
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
    }
}
