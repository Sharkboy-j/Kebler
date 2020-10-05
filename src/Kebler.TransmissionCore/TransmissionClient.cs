using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Models;
using Kebler.Models.Torrent;
using Kebler.Models.Torrent.Args;
using Kebler.Models.Torrent.Common;
using Kebler.Models.Torrent.Entity;
using Kebler.Models.Torrent.Response;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kebler.TransmissionCore
{
    public partial class TransmissionClient
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly string _authorization;
        private readonly bool _needAuthorization;
        private int _numberOfAttempts = 4;


        /// <summary>
        ///     Initialize client
        ///     <example>
        ///         For example
        ///         <code>
        /// new Transmission.API.RPC.Client("https://website.com:9091/transmission/rpc")
        /// </code>
        ///     </example>
        /// </summary>
        /// <param name="url">URL to Transmission RPC API. Often it looks like schema://host:port/transmission/rpc </param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        public TransmissionClient(string url, string sessionId = null, string login = null, string password = null)
        {
            Url = url;
            SessionId = sessionId;

            if (string.IsNullOrWhiteSpace(login)) return;

            var authBytes = Encoding.UTF8.GetBytes(login + ":" + password);
            var encoded = Convert.ToBase64String(authBytes);

            _authorization = "Basic " + encoded;
            _needAuthorization = true;
        }

        /// <summary>
        ///     Url to service
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Session ID
        /// </summary>
        public string SessionId { get; private set; }

        /// <summary>
        ///     Current Tag
        /// </summary>
        public int CurrentTag { get; private set; }

        public int NumberOfAttempts
        {
            get => _numberOfAttempts;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("Value has to be greater than one.");

                _numberOfAttempts = value;
            }
        }

        public static bool CheckURLValid(string source)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out _);
        }
    }


    public partial class TransmissionClient
    {
        private async Task<TransmissionResponse> SendRequestAsync(TransmissionRequest request, CancellationToken token)
        {
            var result = new TransmissionResponse();
            request.Tag = ++CurrentTag;
            var counter = 0;
            var byteArray = Encoding.UTF8.GetBytes(request.ToJson());
            //Prepare http web request
            if (!CheckURLValid(Url)) throw new WebException("Host error", WebExceptionStatus.NameResolutionFailure);


            while (counter < NumberOfAttempts)
                try
                {
                    var webRequest = (HttpWebRequest) WebRequest.Create(Url);
                    webRequest.ContentType = "application/json-rpc";
                    webRequest.Method = "POST";
                    if (_needAuthorization)
                        webRequest.Headers["Authorization"] = _authorization;

                    webRequest.Headers["X-Transmission-Session-Id"] = SessionId;

                    await using (var dataStream = await webRequest.GetRequestStreamAsync())
                    {
                        await dataStream.WriteAsync(byteArray, 0, byteArray.Length, token);
                    }

                    //Send request and prepare response
                    using var webResponse = await webRequest.GetResponseAsync(token);
                    await using var responseStream = await webResponse.GetResponseStream(token);

                    if (responseStream == null)
                    {
                        result.CustomException = new Exception("Stream resonse is null");
                        Log.Error(result.WebException);
                        return result;
                    }

                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var responseString = await reader.ReadToEndAsync(token);
                    result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                    if (result.Result != "success") throw new Exception(result.Result);
                    break;
                }
                catch (TaskCanceledException)
                {
                    result.Result = "canceled";
                    return result;
                }
                catch (WebException ex)
                {
                    result.WebException = ex;
                    if (ex.Response is HttpWebResponse wr)
                    {
                        result.HttpWebResponse = wr;
                        if (result.HttpWebResponse.StatusCode == HttpStatusCode.Conflict)
                            SessionId = ex.Response.Headers["X-Transmission-Session-Id"];
                        else
                            counter++;
                    }
                    else
                    {
                        counter++;
                    }
                }
                catch (Exception ex)
                {
                    result.CustomException = ex;
                    counter++;
                }

            result.Method = request.Method;
            return result;
        }

        #region Session methods

        /// <summary>
        ///     Set information to current session (API: session-set)
        /// </summary>
        public async Task<TransmissionResponse<SessionSettings>> GetSessionSettingsAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("session-get");
            var response = await SendRequestAsync(request, token);
            var resp = new TransmissionResponse<SessionSettings>(response);
            return resp;
        }

        /// <summary>
        ///     Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public Task<TransmissionResponse> SetSessionSettingsAsync(SessionSettings settings, CancellationToken token)
        {
            var request = new TransmissionRequest("session-set", settings);
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Get session stat
        /// </summary>
        /// <returns>Session stat</returns>
        public async Task<TransmissionResponse<Statistic>> GetSessionStatisticAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("session-stats");
            var response = await SendRequestAsync(request, token);
            var trans = new TransmissionResponse<Statistic>(response);
            return trans;
        }

        /// <summary>
        ///     Get information of current session (API: session-get)
        /// </summary>
        /// <returns>Session information</returns>
        public async Task<TransmissionResponse<SessionInfo>> GetSessionInformationAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("session-get");
            var response = await SendRequestAsync(request, token);
            var trans = new TransmissionResponse<SessionInfo>(response);
            return trans;
        }

        #endregion

        #region Torrents methods

        /// <summary>
        ///     Add torrent (API: torrent-add)
        /// </summary>
        /// <returns>Torrent info (ID, Name and HashString)</returns>
        public async Task<AddTorrentResponse> TorrentAddAsync(NewTorrent torrent, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(torrent.Metainfo) && string.IsNullOrWhiteSpace(torrent.Filename))
                throw new Exception("Either \"filename\" or \"metainfo\" must be included.");

            var request = new TransmissionRequest("torrent-add", torrent);
            var response = await SendRequestAsync(request, token);
            var jObject = response.Deserialize<JObject>();

            var result = new AddTorrentResponse(Enums.ReponseResult.NotOk);


            if (jObject?.First == null)
                return result;

            if (jObject.TryGetValue("torrent-duplicate", out var value))
            {
                result = new AddTorrentResponse(Enums.ReponseResult.Ok)
                {
                    Value = JsonConvert.DeserializeObject<TorrentAddResult>(value.ToString())
                };
                result.Value.Status = Enums.AddTorrentStatus.Duplicate;
            }
            else if (jObject.TryGetValue("torrent-added", out value))
            {
                result = new AddTorrentResponse(Enums.ReponseResult.Ok)
                {
                    Value = JsonConvert.DeserializeObject<TorrentAddResult>(value.ToString())
                };
                result.Value.Status = Enums.AddTorrentStatus.Added;
            }


            return result;
        }

        /// <summary>
        ///     Set torrent params (API: torrent-set)
        /// </summary>
        /// <param name="settings">Torrent settings</param>
        public Task<TransmissionResponse> TorrentSetAsync(TorrentSettings settings, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-set", settings);
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public async Task<TransmissionResponse<TransmissionTorrents>> TorrentGetAsync(string[] fields,
            CancellationToken token)
        {
            var arguments = new Dictionary<string, object> {{"fields", fields}};
            var request = new TransmissionRequest("torrent-get", arguments);
            var response = await SendRequestAsync(request, token);
            var trans = new TransmissionResponse<TransmissionTorrents>(response);
            return trans;
        }

        /// <summary>
        ///     Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public async Task<TransmissionTorrents> TorrentGetAsyncWithID(string[] fields, CancellationToken token,
            params uint[] ids)
        {
            var arguments = new Dictionary<string, object> {{"fields", fields}};

            if (ids != null && ids.Length > 0)
                arguments.Add("ids", ids);

            var request = new TransmissionRequest("torrent-get", arguments);

            var response = await SendRequestAsync(request, token);
            var result = response.Deserialize<TransmissionTorrents>();

            return result;
        }

        /// <summary>
        ///     Remove torrents
        /// </summary>
        /// <param name="ids">Torrents id</param>
        /// <param name="deleteData">Remove data</param>
        public async Task<Enums.RemoveResult> TorrentRemoveAsync(uint[] ids, CancellationToken token,
            bool deleteData = false)
        {
            var arguments = new Dictionary<string, object>
            {
                {"ids", ids},
                {"delete-local-data", deleteData}
            };

            var request = new TransmissionRequest("torrent-remove", arguments);
            var response = await SendRequestAsync(request, token);

            var resp = response.WebException == null && response.HttpWebResponse == null &&
                       response.CustomException == null
                ? Enums.RemoveResult.Ok
                : Enums.RemoveResult.Error;

            return resp;
        }

        #region Torrent Start

        /// <summary>
        ///     Start torrents (API: torrent-start)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public async Task<TransmissionResponse> TorrentStartAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-start", new Dictionary<string, object> {{"ids", ids}});
            var resp = await SendRequestAsync(request, token);
            return resp;
        }

        /// <summary>
        ///     Start torrents (API: torrent-start)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentStartForceAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> {{"ids", ids}});
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Start recently active torrents (API: torrent-start)
        /// </summary>
        public Task TorrentStartAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-start",
                new Dictionary<string, object> {{"ids", "recently-active"}});
            return SendRequestAsync(request, token);
        }

        #endregion

        #region Torrent Start Now

        /// <summary>
        ///     Start now torrents (API: torrent-start-now)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentStartNowAsync(int[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> {{"ids", ids}});
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Start now recently active torrents (API: torrent-start-now)
        /// </summary>
        public Task TorrentStartNowAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-start-now",
                new Dictionary<string, object> {{"ids", "recently-active"}});
            return SendRequestAsync(request, token);
        }

        #endregion

        #region Torrent Stop

        /// <summary>
        ///     Stop torrents (API: torrent-stop)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task<TransmissionResponse> TorrentStopAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> {{"ids", ids}});
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Stop recently active torrents (API: torrent-stop)
        /// </summary>
        public Task TorrentStopAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-stop",
                new Dictionary<string, object> {{"ids", "recently-active"}});
            return SendRequestAsync(request, token);
        }

        #endregion

        #region Torrent Verify

        /// <summary>
        ///     Verify torrents (API: torrent-verify)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task<TransmissionResponse> TorrentVerifyAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-verify", new Dictionary<string, object> {{"ids", ids}});
            return SendRequestAsync(request, token);
        }

        /// <summary>
        ///     Verify recently active torrents (API: torrent-verify)
        /// </summary>
        public Task TorrentVerifyAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("torrent-verify",
                new Dictionary<string, object> {{"ids", "recently-active"}});
            return SendRequestAsync(request, token);
        }

        #endregion

        /// <summary>
        ///     Move torrents in queue on top (API: queue-move-top)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async Task<TransmissionResponse> TorrentQueueMoveTopAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("queue-move-top", new Dictionary<string, object> {{"ids", ids}});
            var resp = await SendRequestAsync(request, token);
            return resp;
        }

        /// <summary>
        ///     Move up torrents in queue (API: queue-move-up)
        /// </summary>
        /// <param name="ids"></param>
        public async Task<TransmissionResponse> TorrentQueueMoveUpAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("queue-move-up", new Dictionary<string, object> {{"ids", ids}});
            var resp = await SendRequestAsync(request, token);
            return resp;
        }

        /// <summary>
        ///     Move down torrents in queue (API: queue-move-down)
        /// </summary>
        /// <param name="ids"></param>
        public async Task<TransmissionResponse> TorrentQueueMoveDownAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("queue-move-down", new Dictionary<string, object> {{"ids", ids}});
            var resp = await SendRequestAsync(request, token);
            return resp;
        }

        /// <summary>
        ///     Move torrents to bottom in queue  (API: queue-move-bottom)
        /// </summary>
        /// <param name="ids"></param>
        public async Task<TransmissionResponse> TorrentQueueMoveBottomAsync(uint[] ids, CancellationToken token)
        {
            var request = new TransmissionRequest("queue-move-bottom", new Dictionary<string, object> {{"ids", ids}});
            var resp = await SendRequestAsync(request, token);
            return resp;
        }

        /// <summary>
        ///     Set new location for torrents files (API: torrent-set-location)
        /// </summary>
        /// <param name="ids">Torrent ids</param>
        /// <param name="location">The new torrent location</param>
        /// <param name="move">Move from previous location</param>
        public async Task<TransmissionResponse> TorrentSetLocationAsync(uint[] ids, string location, bool move,
            CancellationToken token)
        {
            var arguments = new Dictionary<string, object> {{"ids", ids}, {"location", location}, {"move", move}};
            var request = new TransmissionRequest("torrent-set-location", arguments);
            var response = await SendRequestAsync(request, token);
            return response;
        }

        /// <summary>
        ///     Rename a file or directory in a torrent (API: torrent-rename-path)
        /// </summary>
        /// <param name="id">The torrent whose path will be renamed</param>
        /// <param name="path">The path to the file or folder that will be renamed</param>
        /// <param name="name">The file or folder's new name</param>
        public async Task<TransmissionResponse<RenameTorrentInfo>> TorrentRenamePathAsync(uint id, string path,
            string name, CancellationToken token)
        {
            var arguments = new Dictionary<string, object> {{"ids", new[] {id}}, {"path", path}, {"name", name}};
            var request = new TransmissionRequest("torrent-rename-path", arguments);
            var response = await SendRequestAsync(request, token);
            var trans = new TransmissionResponse<RenameTorrentInfo>(response);
            return trans;
        }

        //method name not recognized
        /// <summary>
        ///     Reannounce torrent (API: torrent-reannounce)
        /// </summary>
        /// <param name="ids"></param>
        public async Task<TransmissionResponse> ReannounceTorrentsAsync(uint[] ids, CancellationToken token)
        {
            var arguments = new Dictionary<string, object> {{"ids", ids}};

            var request = new TransmissionRequest("torrent-reannounce", arguments);
            var response = await SendRequestAsync(request, token);
            return response;
        }

        #endregion

        #region System

        /// <summary>
        ///     See if your incoming peer port is accessible from the outside world (API: port-test)
        /// </summary>
        /// <returns>Accessible state</returns>
        public async Task<bool> PortTestAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("port-test");
            var response = await SendRequestAsync(request, token);

            var data = response.Deserialize<JObject>();
            var result = (bool) data.GetValue("port-is-open");
            return result;
        }

        /// <summary>
        ///     Update blocklist (API: blocklist-update)
        /// </summary>
        /// <returns>Blocklist size</returns>
        public async Task<int> BlocklistUpdateAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("blocklist-update");
            var response = await SendRequestAsync(request, token);

            var data = response.Deserialize<JObject>();
            var result = (int) data.GetValue("blocklist-size");
            return result;
        }

        /// <summary>
        ///     Get free space is available in a client-specified folder.
        /// </summary>
        /// <param name="path">The directory to query</param>
        public async Task<long> FreeSpaceAsync(string path, CancellationToken token)
        {
            var arguments = new Dictionary<string, object> {{"path", path}};

            var request = new TransmissionRequest("free-space", arguments);
            var response = await SendRequestAsync(request, token);

            var data = response.Deserialize<JObject>();
            var result = (long) data.GetValue("size-bytes");
            return result;
        }

        #endregion


        //catch (WebException ex)
        //{
        //    result.WebException = ex;
        //    if (ex.Response is HttpWebResponse wr)
        //    {
        //        result.HttpWebResponse = wr;

        //        if (result.HttpWebResponse.StatusCode == HttpStatusCode.Conflict)
        //        {
        //            if (ex.Response.Headers.Count > 0)
        //            {
        //                result.WebException = null;
        //                //If session id expiried, try get session id and send request
        //                SessionId = ex.Response.Headers["X-Transmission-Session-Id"];

        //                if (SessionId == null)
        //                {
        //                    result.CustomException = new Exception("Session ID Error");
        //                    result.Method = request.Method;
        //                    return result;
        //                }
        //                result = await SendRequestAsync(request, token);
        //                result.Method = request.Method;
        //                return result;
        //            }
        //        }
        //    }
        //    Log.Error(ex);


        //}
        //catch (Exception ex)
        //{
        //    result.CustomException = ex;
        //    Log.Error(ex);
        //}
    }
}