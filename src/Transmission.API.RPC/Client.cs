using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Transmission.API.RPC.Entity;
using Newtonsoft.Json.Linq;
using Transmission.API.RPC.Common;
using Transmission.API.RPC.Arguments;
using static Transmission.API.RPC.Entity.Enums;
using log4net;
using System.Reflection;

namespace Transmission.API.RPC
{
    public class TransmissionClient
    {

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public string Host
        {
            get;
            private set;
        }

        public string SessionID
        {
            get;
            private set;
        }

        public int CurrentTag
        {
            get;
            private set;
        }

        private string _authorization;
        private bool _needAuthorization;

        /// <summary>
        /// miliseconds
        /// </summary>
        public int timeOut = 10000;

        /// <summary>
        /// Initialize client
        /// </summary>
        /// <param name="host">Host adresse</param>
        /// <param name="sessionID">Session ID</param>
        /// <param name="login">Login</param>
        /// <param name="password">Password</param>
        /// <exception cref="UriFormatException"></exception>
        public TransmissionClient(string host, string sessionID = null, string login = null, string password = null)
        {
            var res = Uri.TryCreate(host, UriKind.Absolute, out var tt);

            if (!res) throw new UriFormatException($"Unable cast {host} as uri");

            this.Host = host;
            this.SessionID = sessionID;

            if (!String.IsNullOrWhiteSpace(login))
            {
                var authBytes = Encoding.UTF8.GetBytes(login + ":" + password);
                var encoded = Convert.ToBase64String(authBytes);
                this._authorization = "Basic " + encoded;
                this._needAuthorization = true;
            }
        }

        public async Task<T> SendRequestAsync<T>(string proc)
        {
            var request = new TransmissionRequest(proc);
            var response = await SendRequestAsync(request);
            var result = response.Deserialize<T>();
            return result;
        }

        public Task SendRequestAsync<T>(string proc, ArgumentsBase arguments)
        {
            var request = new TransmissionRequest(proc, arguments);
            return SendRequestAsync(request);
            //var request = new TransmissionRequest(proc);
            //var response = await SendRequestAsync(request);
            //var result = response.Deserialize<T>();
            //return result;
        }

        public async Task<T> SendRequestAsync<T>(string proc, Dictionary<string, object> arguments)
        {
            var request = new TransmissionRequest(proc, arguments);
            var response = await SendRequestAsync(request);
            var result = response.Deserialize<T>();
            return result;
            //var request = new TransmissionRequest(proc);
            //var response = await SendRequestAsync(request);
            //var result = response.Deserialize<T>();
            //return result;
        }


        #region Session methods

        /// <summary>
        /// Close current session (API: session-close)
        /// </summary>
        public Task CloseSessionAsync()
        {
            return SendRequestAsync<TransmissionResponse>("session-close");
            //var request = new TransmissionRequest("session-close");
            //var response = SendRequest(request);
        }

        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public Task SetSessionSettingsAsync(SessionSettings settings)
        {
            return SendRequestAsync<TransmissionResponse>("session-set", settings);

            //var request = new TransmissionRequest("session-set", settings);
            //var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public Task<SessionSettings> GetSessionSettingsAsync()
        {
            return SendRequestAsync<SessionSettings>("session-get");

            //var request = new TransmissionRequest("session-get");
            //var response = await SendRequestAsync(request);
            //var result = response?.Deserialize<SessionSettings>();
            //return result;
        }


        /// <summary>
        /// Get session stat
        /// </summary>
        /// <returns>Session stat</returns>
        public Task<Statistic> GetSessionStatisticAsync()
        {
            return SendRequestAsync<Statistic>("session-stats");

            //var request = new TransmissionRequest("session-stats");
            //var response = await SendRequestAsync(request);
            //var result = response?.Deserialize<Statistic>();
            //return result;
        }


        /// <summary>
        /// Get information of current session (API: session-get)
        /// </summary>
        /// <returns>Session information</returns>
        /// <exception cref="WebException"
        /// <exception cref="Exception"
        public Task<SessionInfo> GetSessionInformationAsync()
        {
            return SendRequestAsync<SessionInfo>("session-get");

            //var resp = await SendRequestAsync<SessionInfo>("session-get");
            //return resp;
        }



        #endregion

        #region Torrents methods

        /// <summary>
        /// Add torrent (API: torrent-add)
        /// </summary>
        /// <returns>Torrent info (ID, Name and HashString)</returns>
        /// <exception cref="Exception"
        public async Task<TorrentAddResult> TorrentAddAsync(NewTorrent torrent)
        {

            if (string.IsNullOrWhiteSpace(torrent.Metainfo) && String.IsNullOrWhiteSpace(torrent.Filename))
                throw new Exception("Either \"filename\" or \"metainfo\" must be included.");

            var request = new TransmissionRequest("torrent-add", torrent);
            var response = await SendRequestAsync(request);
            var jObject = response?.Deserialize<JObject>();

            if (jObject == null || jObject.First == null)
                return new TorrentAddResult(AddResult.ResponseNull);

            if (jObject.TryGetValue("torrent-duplicate", out var value))
            {
                return new TorrentAddResult(AddResult.Duplicate);
            }
            else if (jObject.TryGetValue("torrent-added", out value))
            {
                var result = JsonConvert.DeserializeObject<TorrentAddResult>(value.ToString());
                result.Result = AddResult.Added;
                return result;
            }
            return new TorrentAddResult(AddResult.Error);
        }


        /// <summary>
        /// Set torrent params (API: torrent-set)
        /// </summary>
        /// <param name="torrentSet">New torrent params</param>
        public Task TorrentSetAsync(TorrentSettings settings)
        {
            return SendRequestAsync<SessionInfo>("torrent-set", settings);

            //var request = new TransmissionRequest("torrent-set", settings);
            //return SendRequestAsync(request);
        }


        /// <summary>
        /// Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public Task<TransmissionTorrents> TorrentGetAsync(/*string[] fields, params int[] ids*/)
        {
            var fields = TorrentFields.ALL_FIELDS;
            var arguments = new Dictionary<string, object> { { "fields", fields } };

            //if (ids != null && ids.Length > 0)
            //arguments.Add("ids", ids);

            return SendRequestAsync<TransmissionTorrents>("torrent-get", arguments);

            // return new TransmissionTorrents();
            //var request = new TransmissionRequest("torrent-get", arguments);

            //var response = await SendRequestAsync(request);
            //var result = response?.Deserialize<TransmissionTorrents>();

            //return result;
        }


        /// <summary>
        /// Remove torrents
        /// </summary>
        /// <param name="ids">Torrents id</param>
        /// <param name="deleteLocalData">Remove local data</param>
        public Task<RemoveResult> TorrentRemoveAsync(int[] ids, bool deleteData = false)
        {
            var arguments = new Dictionary<string, object> { { "ids", ids }, { "delete-local-data", deleteData } };

            return SendRequestAsync<RemoveResult>("torrent-remove", arguments);

            //var request = new TransmissionRequest("torrent-remove", arguments);
            //var response = await SendRequestAsync(request);

            //return resp.Result == "success" ? RemoveResult.Ok : RemoveResult.Error;
        }


        /// <summary>
        /// Start torrents (API: torrent-start)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async void TorrentStartAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }


        /// <summary>
        /// Start now torrents (API: torrent-start-now)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async void TorrentStartNowAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);

        }

        /// <summary>
        /// Stop torrents (API: torrent-stop)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async Task TorrentStopAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }


        /// <summary>
        /// Verify torrents (API: torrent-verify)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async void TorrentVerifyAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-verify", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }


        /// <summary>
        /// Move torrents in queue on top (API: queue-move-top)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async void TorrentQueueMoveTopAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-top", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }


        /// <summary>
        /// Move up torrents in queue (API: queue-move-up)
        /// </summary>
        /// <param name="ids"></param>
        public async void TorrentQueueMoveUpAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-up", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Move down torrents in queue (API: queue-move-down)
        /// </summary>
        /// <param name="ids"></param>
        public async void TorrentQueueMoveDownAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-down", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Move torrents to bottom in queue  (API: queue-move-bottom)
        /// </summary>
        /// <param name="ids"></param>
        public async void TorrentQueueMoveBottomAsync(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-bottom", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Set new location for torrents files (API: torrent-set-location)
        /// </summary>
        /// <param name="ids">Torrent ids</param>
        /// <param name="location">The new torrent location</param>
        /// <param name="move">Move from previous location</param>
        public async void TorrentSetLocationAsync(int[] ids, string location, bool move)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", ids);
            arguments.Add("location", location);
            arguments.Add("move", move);

            var request = new TransmissionRequest("torrent-set-location", arguments);
            var response = await SendRequestAsync(request);
        }


        /// <summary>
        /// Rename a file or directory in a torrent (API: torrent-rename-path)
        /// </summary>
        /// <param name="ids">The torrent whose path will be renamed</param>
        /// <param name="path">The path to the file or folder that will be renamed</param>
        /// <param name="name">The file or folder's new name</param>
        public async Task<RenameTorrentInfo> TorrentRenamePathAsync(int id, string path, string name)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new int[] { id });
            arguments.Add("path", path);
            arguments.Add("name", name);

            var request = new TransmissionRequest("torrent-rename-path", arguments);
            var response = await SendRequestAsync(request);

            var result = response?.Deserialize<RenameTorrentInfo>();

            return result;
        }

        //method name not recognized
        ///// <summary>
        ///// Reannounce torrent (API: torrent-reannounce)
        ///// </summary>
        ///// <param name="ids"></param>
        //public void ReannounceTorrents(int[] ids)
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

            var data = response?.Deserialize<JObject>();
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

            var data = response?.Deserialize<JObject>();
            var result = (int)data.GetValue("blocklist-size");
            return result;
        }

        /// <summary>
        /// Get free space is available in a client-specified folder.
        /// </summary>
        /// <param name="path">The directory to query</param>
        public async Task<long> FreeSpaceAsync(string path)
        {
            var arguments = new Dictionary<string, object>
            {
                { "path", path }
            };

            var request = new TransmissionRequest("free-space", arguments);
            var response = await SendRequestAsync(request);

            var data = response?.Deserialize<JObject>();
            var result = (long)data.GetValue("size-bytes");
            return result;
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"
        private async Task<TransmissionResponse> SendRequestAsync(TransmissionRequest request)
        {
            var result = new TransmissionResponse();

            try
            {

                request.Tag = ++CurrentTag;

                var byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                var webRequest = (HttpWebRequest)WebRequest.Create(Host);
                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionID;
                webRequest.Method = "POST";
                webRequest.Timeout = timeOut;
                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                using (var dataStream = await webRequest.GetRequestStreamAsync())
                {
                    await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
                }

                //Send request and prepare response
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseString = await reader.ReadToEndAsync();
                        result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                        if (result.Result != "success")
                            throw new Exception(result.Result);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    //Log.Error(ex);
                    result.Error = ErrorsResponse.TimeOut;
                    return result;
                }

                if (ex?.Response is HttpWebResponse res)
                {
                    if (res?.StatusCode == HttpStatusCode.Conflict)
                    {
                        if (res.Headers.Count > 0)
                        {
                            //If session id expiried, try get session id and send request
                            SessionID = res.Headers["X-Transmission-Session-Id"];

                            if (SessionID == null)
                                throw new Exception("Session ID Error");

                            result = await SendRequestAsync(request);
                        }
                    }
                    else
                    {
                        result.Error = ErrorsResponse.HttpStatusCodeConflict;
                    }
                }
                else
                {
                    result.Error = ErrorsResponse.IsNotHttpWebResponse;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return result;
        }
    }
}
