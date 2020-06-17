using Kebler.Models.Torrent.Common;
using log4net;
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
using Kebler.Models.Torrent.Entity;
using Kebler.Models.Torrent.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kebler.TransmissionCore
{
    public partial class TransmissionClient
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly string _authorization;
        private readonly bool _needAuthorization;

        /// <summary>
        /// Url to service
        /// </summary>
        public string Url
        {
            get;
        }

        /// <summary>
        /// Session ID
        /// </summary>
        public string SessionId
        {
            get;
            private set;
        }

        /// <summary>
        /// Current Tag
        /// </summary>
        public int CurrentTag
        {
            get;
            private set;
        }

        /// <summary>
        /// Initialize client
        /// <example>For example
        /// <code>
        /// new Transmission.API.RPC.Client("https://website.com:9091/transmission/rpc")
        /// </code>
        /// </example>
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


        private TransmissionResponse SendRequest(CommunicateBase request)
        {
            var result = new TransmissionResponse();

            request.Tag = ++CurrentTag;

            try
            {

                byte[] byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                var webRequest = (HttpWebRequest)WebRequest.Create(Url);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionId;
                webRequest.Method = "POST";

                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                var requestTask = webRequest.GetRequestStreamAsync();
                requestTask.WaitAndUnwrapException();
                using (var dataStream = requestTask.Result)
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                var responseTask = webRequest.GetResponseAsync();
                responseTask.WaitAndUnwrapException();

                //Send request and prepare response
                using var webResponse = responseTask.Result;
                using var responseStream = webResponse.GetResponseStream();
                var reader = new StreamReader(responseStream ?? throw new InvalidOperationException(nameof(responseStream)), Encoding.UTF8);
                var responseString = reader.ReadToEnd();
                result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                if (result.Result != "success")
                    throw new Exception(result.Result);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    if (ex.Response.Headers.Count > 0)
                    {
                        //If session id expiried, try get session id and send request
                        SessionId = ex.Response.Headers["X-Transmission-Session-Id"];

                        if (SessionId == null)
                            throw new Exception("Session ID Error");

                        result = SendRequest(request);
                    }
                }
                else
                    throw ex;
            }

            return result;
        }
    }



    public partial class TransmissionClient
    {
        #region Session methods

        /// <summary>
        /// Close current session (API: session-close)
        /// </summary>
        public Task CloseSessionAsync()
        {
            var request = new TransmissionRequest("session-close");
            return SendRequestAsync(request);
        }


        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        public async Task<SessionSettings> GetSessionSettingsAsync(CancellationToken token)
        {
            var request = new TransmissionRequest("session-get");
            var response = await SendRequestAsync(request, token);
            var result = response?.Deserialize<SessionSettings>();
            return result;
        }

        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public Task SetSessionSettingsAsync(SessionSettings settings)
        {
            var request = new TransmissionRequest("session-set", settings);
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Get session stat
        /// </summary>
        /// <returns>Session stat</returns>
        public async Task<Statistic> GetSessionStatisticAsync()
        {
            var request = new TransmissionRequest("session-stats");
            var response = await SendRequestAsync(request);
            var result = response.Deserialize<Statistic>();
            return result;
        }

        /// <summary>
        /// Get information of current session (API: session-get)
        /// </summary>
        /// <returns>Session information</returns>
        public async Task<TransmissionInfoResponse<SessionInfo>> GetSessionInformationAsync()
        {
            var request = new TransmissionRequest("session-get");
            var response = await SendRequestAsync(request);
            var result = response.Deserialize<SessionInfo>();

            var answ = new TransmissionInfoResponse<SessionInfo>(result)
            {
                HttpWebResponse = response.HttpWebResponse,
                CustomException = response.CustomException,
                WebException = response.WebException
            };
            return answ;
        }

        #endregion

        #region Torrents methods

        /// <summary>
        /// Add torrent (API: torrent-add)
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
        /// Set torrent params (API: torrent-set)
        /// </summary>
        /// <param name="settings">Torrent settings</param>
        public Task TorrentSetAsync(TorrentSettings settings)
        {
            var request = new TransmissionRequest("torrent-set", settings);
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public async Task<TransmissionTorrents> TorrentGetAsync(string[] fields)
        {
            var arguments = new Dictionary<string, object> { { "fields", fields } };

            var request = new TransmissionRequest("torrent-get", arguments);

            var response = await SendRequestAsync(request);
            var result = response.Deserialize<TransmissionTorrents>();

            return result;
        }

        /// <summary>
        /// Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public async Task<TransmissionTorrents> TorrentGetAsyncWithID(string[] fields, params int[] ids)
        {
            var arguments = new Dictionary<string, object> { { "fields", fields } };

            if (ids != null && ids.Length > 0)
                arguments.Add("ids", ids);

            var request = new TransmissionRequest("torrent-get", arguments);

            var response = await SendRequestAsync(request);
            var result = response.Deserialize<TransmissionTorrents>();

            return result;
        }

        /// <summary>
        /// Remove torrents
        /// </summary>
        /// <param name="ids">Torrents id</param>
        /// <param name="deleteData">Remove data</param>
        public async Task<Enums.RemoveResult> TorrentRemoveAsync(int[] ids, CancellationToken token, bool deleteData = false)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", ids },
                { "delete-local-data", deleteData }
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
        /// Start torrents (API: torrent-start)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentStartAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Start recently active torrents (API: torrent-start)
        /// </summary>
        public Task TorrentStartAsync()
        {
            var request = new TransmissionRequest("torrent-start", new Dictionary<string, object> { { "ids", "recently-active" } });
            return SendRequestAsync(request);
        }

        #endregion

        #region Torrent Start Now

        /// <summary>
        /// Start now torrents (API: torrent-start-now)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentStartNowAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Start now recently active torrents (API: torrent-start-now)
        /// </summary>
        public Task TorrentStartNowAsync()
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> { { "ids", "recently-active" } });
            return SendRequestAsync(request);
        }

        #endregion

        #region Torrent Stop

        /// <summary>
        /// Stop torrents (API: torrent-stop)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentStopAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Stop recently active torrents (API: torrent-stop)
        /// </summary>
        public Task TorrentStopAsync()
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> { { "ids", "recently-active" } });
            return SendRequestAsync(request);
        }

        #endregion

        #region Torrent Verify

        /// <summary>
        /// Verify torrents (API: torrent-verify)
        /// </summary>
        /// <param name="ids">A list of torrent id numbers, sha1 hash strings, or both</param>
        public Task TorrentVerifyAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-verify", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Verify recently active torrents (API: torrent-verify)
        /// </summary>
        public Task TorrentVerifyAsync()
        {
            var request = new TransmissionRequest("torrent-verify", new Dictionary<string, object> { { "ids", "recently-active" } });
            return SendRequestAsync(request);
        }
        #endregion

        /// <summary>
        /// Move torrents in queue on top (API: queue-move-top)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public Task TorrentQueueMoveTopAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-top", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Move up torrents in queue (API: queue-move-up)
        /// </summary>
        /// <param name="ids"></param>
        public Task TorrentQueueMoveUpAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-up", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Move down torrents in queue (API: queue-move-down)
        /// </summary>
        /// <param name="ids"></param>
        public Task TorrentQueueMoveDownAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-down", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Move torrents to bottom in queue  (API: queue-move-bottom)
        /// </summary>
        /// <param name="ids"></param>
        public Task TorrentQueueMoveBottomAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-bottom", new Dictionary<string, object> { { "ids", ids } });
            return SendRequestAsync(request);
        }

        /// <summary>
        /// Set new location for torrents files (API: torrent-set-location)
        /// </summary>
        /// <param name="ids">Torrent ids</param>
        /// <param name="location">The new torrent location</param>
        /// <param name="move">Move from previous location</param>
        public async Task<TransmissionResponse> TorrentSetLocationAsync(int[] ids, string location, bool move, CancellationToken token)
        {
            var arguments = new Dictionary<string, object> { { "ids", ids }, { "location", location }, { "move", move } };
            var request = new TransmissionRequest("torrent-set-location", arguments);
            var response = await SendRequestAsync(request, token);
            return response;
        }

        /// <summary>
        /// Rename a file or directory in a torrent (API: torrent-rename-path)
        /// </summary>
        /// <param name="id">The torrent whose path will be renamed</param>
        /// <param name="path">The path to the file or folder that will be renamed</param>
        /// <param name="name">The file or folder's new name</param>
        public async Task<RenameTorrentInfo> TorrentRenamePathAsync(int id, string path, string name)
        {
            var arguments = new Dictionary<string, object> { { "ids", new[] { id } }, { "path", path }, { "name", name } };

            var request = new TransmissionRequest("torrent-rename-path", arguments);
            var response = await SendRequestAsync(request);

            var result = response.Deserialize<RenameTorrentInfo>();

            return result;
        }

        //method name not recognized
        ///// <summary>
        ///// Reannounce torrent (API: torrent-reannounce)
        ///// </summary>
        ///// <param name="ids"></param>
        //public void ReannounceTorrents(object[] ids)
        //{
        //    var arguments = new Dictionary<string, object>();
        //    arguments.Add("ids", ids);

        //    var request = new TransmissionRequest("torrent-reannounce", arguments);
        //    var response = SendRequest(request);
        //}

        #endregion

        #region System

        /// <summary>
        /// See if your incoming peer port is accessible from the outside world (API: port-test)
        /// </summary>
        /// <returns>Accessible state</returns>
        public async Task<bool> PortTestAsync()
        {
            var request = new TransmissionRequest("port-test");
            var response = await SendRequestAsync(request);

            var data = response.Deserialize<JObject>();
            var result = (bool)data.GetValue("port-is-open");
            return result;
        }

        /// <summary>
        /// Update blocklist (API: blocklist-update)
        /// </summary>
        /// <returns>Blocklist size</returns>
        public async Task<int> BlocklistUpdateAsync()
        {
            var request = new TransmissionRequest("blocklist-update");
            var response = await SendRequestAsync(request);

            var data = response.Deserialize<JObject>();
            var result = (int)data.GetValue("blocklist-size");
            return result;
        }

        /// <summary>
        /// Get free space is available in a client-specified folder.
        /// </summary>
        /// <param name="path">The directory to query</param>
        public async Task<long> FreeSpaceAsync(string path)
        {
            var arguments = new Dictionary<string, object> { { "path", path } };

            var request = new TransmissionRequest("free-space", arguments);
            var response = await SendRequestAsync(request);

            var data = response.Deserialize<JObject>();
            var result = (long)data.GetValue("size-bytes");
            return result;
        }

        #endregion

        private async Task<TransmissionResponse> SendRequestAsync(TransmissionRequest request, CancellationToken token)
        {
            var result = new TransmissionResponse();

            request.Tag = ++CurrentTag;

            try
            {

                var byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                var webRequest = (HttpWebRequest)WebRequest.Create(Url);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionId;
                webRequest.Method = "POST";

                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                await using (var dataStream = await webRequest.GetRequestStreamAsync())
                {
                    await dataStream.WriteAsync(byteArray, 0, byteArray.Length, token);
                }

                //Send request and prepare response
                using var webResponse = await webRequest.GetResponseAsync();
                await using var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                {
                    result.CustomException = new Exception("Stream resonse is null");
                    Log.Error(result.WebException);
                    return result;
                }

                var reader = new StreamReader(responseStream, Encoding.UTF8);
                var responseString = await reader.ReadToEndAsync();
                result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                if (result.Result != "success")
                {
                    throw new Exception(result.Result);
                }
            }
            catch (WebException ex)
            {
                result.WebException = ex;
                if (ex.Response is HttpWebResponse wr)
                {
                    result.HttpWebResponse = wr;

                    if (result.HttpWebResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        if (ex.Response.Headers.Count > 0)
                        {
                            //If session id expiried, try get session id and send request
                            SessionId = ex.Response.Headers["X-Transmission-Session-Id"];

                            if (SessionId == null)
                            {
                                result.CustomException = new Exception("Session ID Error");
                                return result;
                            }
                            result = SendRequest(request);
                        }
                    }
                }

                Log.Error(ex);
            }
            catch (Exception ex)
            {
                result.CustomException = ex;
                Log.Error(ex);
            }

            return result;
        }

        private async Task<TransmissionResponse> SendRequestAsync(TransmissionRequest request)
        {
            var result = new TransmissionResponse();

            request.Tag = ++CurrentTag;

            try
            {

                var byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                var webRequest = (HttpWebRequest)WebRequest.Create(Url);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionId;
                webRequest.Method = "POST";

                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                await using (var dataStream = await webRequest.GetRequestStreamAsync())
                {
                    await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
                }

                //Send request and prepare response
                using var webResponse = await webRequest.GetResponseAsync();
                await using var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                {
                    result.CustomException = new Exception("Stream resonse is null");
                    Log.Error(result.WebException);
                    return result;
                }

                var reader = new StreamReader(responseStream, Encoding.UTF8);
                var responseString = await reader.ReadToEndAsync();
                result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                if (result.Result != "success")
                {
                    throw new Exception(result.Result);
                }
            }
            catch (WebException ex)
            {
                result.WebException = ex;
                if (ex.Response is HttpWebResponse wr)
                {
                    result.HttpWebResponse = wr;

                    if (result.HttpWebResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        if (ex.Response.Headers.Count > 0)
                        {
                            //If session id expiried, try get session id and send request
                            SessionId = ex.Response.Headers["X-Transmission-Session-Id"];

                            if (SessionId == null)
                            {
                                result.CustomException = new Exception("Session ID Error");
                                return result;
                            }
                            result = SendRequest(request);
                        }
                    }
                }
                else
                {
                    Log.Error(ex);
                }

            }
            catch (Exception ex)
            {
                result.CustomException = ex;
                Log.Error(ex);
            }

            return result;
        }
    }
}
