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

namespace Transmission.API.RPC
{
    public class TransmissionClient
    {
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
		/// Initialize client
		/// </summary>
		/// <param name="host">Host adresse</param>
		/// <param name="sessionID">Session ID</param>
		/// <param name="login">Login</param>
		/// <param name="password">Password</param>
		/// <exception cref="UriFormatException"></exception>
		public TransmissionClient(string host, string sessionID = null, string login = null, string password = null)
        {
            var res = Uri.TryCreate(host,UriKind.Absolute,out var tt);

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

        #region Session methods

        /// <summary>
        /// Close current session (API: session-close)
        /// </summary>
        public void CloseSession()
        {
            var request = new TransmissionRequest("session-close");
            var response = SendRequest(request);
        }

        /// <summary>
        /// Close current session (API: session-close)
        /// </summary>
        public async void CloseSessionAsync()
        {
            var request = new TransmissionRequest("session-close");
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public void SetSessionSettings(SessionSettings settings)
        {
            var request = new TransmissionRequest("session-set", settings);
            var response = SendRequest(request);
        }

        /// <summary>
        /// Set information to current session (API: session-set)
        /// </summary>
        /// <param name="settings">New session settings</param>
        public async void SetSessionSettingsAsync(SessionSettings settings)
        {
            var request = new TransmissionRequest("session-set", settings);
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Get session stat
        /// </summary>
        /// <returns>Session stat</returns>
        public Statistic GetSessionStatistic()
        {
            var request = new TransmissionRequest("session-stats");
            var response = SendRequest(request);
            var result = response.Deserialize<Statistic>();
            return result;
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
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        public SessionInfo GetSessionInformation()
        {
            var request = new TransmissionRequest("session-get");
            var response = SendRequest(request);
            var result = response.Deserialize<SessionInfo>();
            return result;
        }

        /// <summary>
        /// Get information of current session (API: session-get)
        /// </summary>
        /// <returns>Session information</returns>
        public async Task<SessionInfo> GetSessionInformationAsync()
        {
            var request = new TransmissionRequest("session-get");
            var response = await SendRequestAsync(request);
            var result = response.Deserialize<SessionInfo>();
            return result;
        }

        #endregion

        #region Torrents methods

        /// <summary>
        /// Add torrent (API: torrent-add)
        /// </summary>
        /// <returns>Torrent info (ID, Name and HashString)</returns>
		public NewTorrentInfo TorrentAdd(NewTorrent torrent)
        {
            if (String.IsNullOrWhiteSpace(torrent.Metainfo) && String.IsNullOrWhiteSpace(torrent.Filename))
                throw new Exception("Either \"filename\" or \"metainfo\" must be included.");

            var request = new TransmissionRequest("torrent-add", torrent);
            var response = SendRequest(request);
            var jObject = response.Deserialize<JObject>();

            if (jObject == null || jObject.First == null)
                return null;

            NewTorrentInfo result = null;
            JToken value = null;

            if (jObject.TryGetValue("torrent-duplicate", out value))
                result = JsonConvert.DeserializeObject<NewTorrentInfo>(value.ToString());
            else if (jObject.TryGetValue("torrent-added", out value))
                result = JsonConvert.DeserializeObject<NewTorrentInfo>(value.ToString());

            return result;
        }

        /// <summary>
        /// Add torrent (API: torrent-add)
        /// </summary>
        /// <returns>Torrent info (ID, Name and HashString)</returns>
        public async Task<NewTorrentInfo> TorrentAddAsync(NewTorrent torrent)
        {
            if (String.IsNullOrWhiteSpace(torrent.Metainfo) && String.IsNullOrWhiteSpace(torrent.Filename))
                throw new Exception("Either \"filename\" or \"metainfo\" must be included.");

            var request = new TransmissionRequest("torrent-add", torrent);
            var response = await SendRequestAsync(request);
            var jObject = response.Deserialize<JObject>();

            if (jObject == null || jObject.First == null)
                return null;

            NewTorrentInfo result = null;
            JToken value = null;

            if (jObject.TryGetValue("torrent-duplicate", out value))
                result = JsonConvert.DeserializeObject<NewTorrentInfo>(value.ToString());
            else if (jObject.TryGetValue("torrent-added", out value))
                result = JsonConvert.DeserializeObject<NewTorrentInfo>(value.ToString());

            return result;
        }

        /// <summary>
        /// Set torrent params (API: torrent-set)
        /// </summary>
        /// <param name="torrentSet">New torrent params</param>
        public void TorrentSet(TorrentSettings settings)
        {
            var request = new TransmissionRequest("torrent-set", settings);
            var response = SendRequest(request);
        }

        /// <summary>
        /// Set torrent params (API: torrent-set)
        /// </summary>
        /// <param name="torrentSet">New torrent params</param>
        public async void TorrentSetAsync(TorrentSettings settings)
        {
            var request = new TransmissionRequest("torrent-set", settings);
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public TransmissionTorrents TorrentGet(string[] fields, params int[] ids)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("fields", fields);

            if (ids != null && ids.Length > 0)
                arguments.Add("ids", ids);

            var request = new TransmissionRequest("torrent-get", arguments);

            var response = SendRequest(request);
            var result = response.Deserialize<TransmissionTorrents>();

            return result;
        }

        /// <summary>
        /// Get fields of torrents from ids (API: torrent-get)
        /// </summary>
        /// <param name="fields">Fields of torrents</param>
        /// <param name="ids">IDs of torrents (null or empty for get all torrents)</param>
        /// <returns>Torrents info</returns>
        public async Task<TransmissionTorrents> TorrentGetAsync(string[] fields, params int[] ids)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("fields", fields);

            if (ids != null && ids.Length > 0)
                arguments.Add("ids", ids);

            var request = new TransmissionRequest("torrent-get", arguments);

            var response = await SendRequestAsync(request);
            var result = response.Deserialize<TransmissionTorrents>();

            return result;
        }

        /// <summary>
        /// Remove torrents (API: torrent-remove)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        /// <param name="deleteLocalData">Remove local data</param>
        public void TorrentRemove(int[] ids, bool deleteData = false)
        {
            var arguments = new Dictionary<string, object>();

            arguments.Add("ids", ids);
            arguments.Add("delete-local-data", deleteData);

            var request = new TransmissionRequest("torrent-remove", arguments);
            var response = SendRequest(request);
        }

        /// <summary>
        /// Remove torrents
        /// </summary>
        /// <param name="ids">Torrents id</param>
        /// <param name="deleteLocalData">Remove local data</param>
        public async void TorrentRemoveAsync(int[] ids, bool deleteData = false)
        {
            var arguments = new Dictionary<string, object>();

            arguments.Add("ids", ids);
            arguments.Add("delete-local-data", deleteData);

            var request = new TransmissionRequest("torrent-remove", arguments);
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Start torrents (API: torrent-start)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public void TorrentStart(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentStartNow(int[] ids)
        {
            var request = new TransmissionRequest("torrent-start-now", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentStop(int[] ids)
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
        }

        /// <summary>
        /// Stop torrents (API: torrent-stop)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public async void TorrentStopAsync(int[] ids)
        {
            var request = new TransmissionRequest("torrent-stop", new Dictionary<string, object> { { "ids", ids } });
            var response = await SendRequestAsync(request);
        }

        /// <summary>
        /// Verify torrents (API: torrent-verify)
        /// </summary>
        /// <param name="ids">Torrents id</param>
        public void TorrentVerify(int[] ids)
        {
            var request = new TransmissionRequest("torrent-verify", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentQueueMoveTop(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-top", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentQueueMoveUp(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-up", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentQueueMoveDown(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-down", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentQueueMoveBottom(int[] ids)
        {
            var request = new TransmissionRequest("queue-move-bottom", new Dictionary<string, object> { { "ids", ids } });
            var response = SendRequest(request);
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
        public void TorrentSetLocation(int[] ids, string location, bool move)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", ids);
            arguments.Add("location", location);
            arguments.Add("move", move);

            var request = new TransmissionRequest("torrent-set-location", arguments);
            var response = SendRequest(request);
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
		public RenameTorrentInfo TorrentRenamePath(int id, string path, string name)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("ids", new int[] { id });
            arguments.Add("path", path);
            arguments.Add("name", name);

            var request = new TransmissionRequest("torrent-rename-path", arguments);
            var response = SendRequest(request);

			var result = response.Deserialize<RenameTorrentInfo>();

			return result;
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

            var result = response.Deserialize<RenameTorrentInfo>();

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
        public bool PortTest()
        {
            var request = new TransmissionRequest("port-test");
            var response = SendRequest(request);

            var data = response.Deserialize<JObject>();
            var result = (bool)data.GetValue("port-is-open");
            return result;
        }

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
        public int BlocklistUpdate()
        {
            var request = new TransmissionRequest("blocklist-update");
            var response = SendRequest(request);

            var data = response.Deserialize<JObject>();
            var result = (int)data.GetValue("blocklist-size");
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
        public long FreeSpace(string path)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("path", path);

            var request = new TransmissionRequest("free-space", arguments);
            var response = SendRequest(request);

            var data = response.Deserialize<JObject>();
            var result = (long)data.GetValue("size-bytes");
            return result;
        }

        /// <summary>
        /// Get free space is available in a client-specified folder.
        /// </summary>
        /// <param name="path">The directory to query</param>
        public async Task<long> FreeSpaceAsync(string path)
        {
            var arguments = new Dictionary<string, object>();
            arguments.Add("path", path);

            var request = new TransmissionRequest("free-space", arguments);
            var response = await SendRequestAsync(request);

            var data = response.Deserialize<JObject>();
            var result = (long)data.GetValue("size-bytes");
            return result;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="Exception"></exception>
        private TransmissionResponse SendRequest(TransmissionRequest request)
        {
            TransmissionResponse result = new TransmissionResponse();

            request.Tag = ++CurrentTag;

            try
            {

                byte[] byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionID;
                webRequest.Method = "POST";
                
                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                var requestTask = webRequest.GetRequestStreamAsync();
                requestTask.WaitAndUnwrapException();
                using (Stream dataStream = requestTask.Result)
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                var responseTask = webRequest.GetResponseAsync();
                responseTask.WaitAndUnwrapException();
                
                //Send request and prepare response
                using (var webResponse = responseTask.Result)
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseString = reader.ReadToEnd();
                        result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                        if (result.Result != "success")
                            throw new Exception(result.Result);
                    }
                }
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    if (ex.Response.Headers.Count > 0)
                    {
                        //If session id expiried, try get session id and send request
                        SessionID = ex.Response.Headers["X-Transmission-Session-Id"];

                        if (SessionID == null)
                            throw new Exception("Session ID Error");

                        result = SendRequest(request);
                    }
                }
                else
                    throw ex;
            }

            return result;
        }

        private async Task<TransmissionResponse> SendRequestAsync(TransmissionRequest request)
        {
            TransmissionResponse result = new TransmissionResponse();

            request.Tag = ++CurrentTag;

            try
            {

                byte[] byteArray = Encoding.UTF8.GetBytes(request.ToJson());

                //Prepare http web request
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Host);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Headers["X-Transmission-Session-Id"] = SessionID;
                webRequest.Method = "POST";

                if (_needAuthorization)
                    webRequest.Headers["Authorization"] = _authorization;

                using (Stream dataStream = await webRequest.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                //Send request and prepare response
                using (var webResponse = await webRequest.GetResponseAsync())
                {
                    using (Stream responseStream = webResponse.GetResponseStream())
                    {
                        var reader = new StreamReader(responseStream, Encoding.UTF8);
                        var responseString = reader.ReadToEnd();
                        result = JsonConvert.DeserializeObject<TransmissionResponse>(responseString);

                        if (result.Result != "success")
                            throw new Exception(result.Result);
                    }
                }
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Conflict)
                {
                    if (ex.Response.Headers.Count > 0)
                    {
                        //If session id expiried, try get session id and send request
                        SessionID = ex.Response.Headers["X-Transmission-Session-Id"];

                        if (SessionID == null)
                            throw new Exception("Session ID Error");

                        result = await SendRequestAsync(request);
                    }
                }
                else
                    throw ex;
            }

            return result;
        }
    }
}
