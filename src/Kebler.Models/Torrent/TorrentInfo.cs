using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kebler.Models.Torrent.Attributes;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TorrentInfo : IComparable
    {
        [JsonConstructor]
        public TorrentInfo(uint id)
        {
            Id = id;
        }

        [JsonProperty(TorrentFields.ID)]
        [SetIgnore]
        public uint Id { get; }

        [JsonProperty(TorrentFields.ADDED_DATE)]
        [SetIgnore]
        public long AddedDate { get; set; }

        [JsonProperty(TorrentFields.BANDWIDTH_PRIORITY)]
        public int BandwidthPriority { get; set; }

        [JsonProperty(TorrentFields.COMMENT)] public string Comment { get; set; }

        [JsonProperty(TorrentFields.CORRUPT_EVER)]
        public int CorruptEver { get; set; }

        [JsonProperty(TorrentFields.CREATOR)] public string Creator { get; set; }

        [JsonProperty(TorrentFields.DATE_CREATED)]
        [SetIgnore]
        public int DateCreated { get; set; }

        [JsonProperty(TorrentFields.DESIRED_AVAILABLE)]
        public long DesiredAvailable { get; set; }

        [JsonProperty(TorrentFields.DONE_DATE)]
        public long DoneDate { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_DIR)]
        public string DownloadDir { get; set; }

        [JsonProperty(TorrentFields.DOWNLOADED_EVER)]
        public long DownloadedEver { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_LIMIT)]
        public long DownloadLimit { get; set; }

        [JsonProperty(TorrentFields.DOWNLOAD_LIMITED)]
        public bool DownloadLimited { get; set; }

        [JsonProperty(TorrentFields.ERROR)] public int Error { get; set; }

        [JsonProperty(TorrentFields.ERROR_STRING)]
        public string ErrorString { get; set; }

        [JsonProperty(TorrentFields.ETA)] public int ETA { get; set; }

        [JsonProperty(TorrentFields.ETA_IDLE)] public int ETAIdle { get; set; }

        [JsonProperty(TorrentFields.FILES)] public TransmissionTorrentFiles[] Files { get; set; }

        [JsonProperty(TorrentFields.FILE_STATS)]
        public TransmissionTorrentFileStats[] FileStats { get; set; }

        [JsonProperty(TorrentFields.HASH_STRING)]
        public string HashString { get; set; }

        [JsonProperty(TorrentFields.HAVE_UNCHECKED)]
        public int HaveUnchecked { get; set; }

        [JsonProperty(TorrentFields.HAVE_VALID)]
        public long HaveValid { get; set; }

        [JsonProperty(TorrentFields.HONORS_SESSION_LIMITS)]
        public bool HonorsSessionLimits { get; set; }

        [JsonProperty(TorrentFields.IS_FINISHED)]
        public bool IsFinished { get; set; }

        [JsonProperty(TorrentFields.IS_PRIVATE)]
        public bool IsPrivate { get; set; }

        [JsonProperty(TorrentFields.IS_STALLED)]
        public bool IsStalled { get; set; }

        [JsonProperty(TorrentFields.LEFT_UNTIL_DONE)]
        public long LeftUntilDone { get; set; }

        [JsonProperty(TorrentFields.MAGNET_LINK)]
        public string MagnetLink { get; set; }

        [JsonProperty(TorrentFields.MANUAL_ANNOUNCE_TIME)]
        public int ManualAnnounceTime { get; set; }

        [JsonProperty(TorrentFields.MAX_CONNECTED_PEERS)]
        public int MaxConnectedPeers { get; set; }

        [JsonProperty(TorrentFields.METADATA_PERCENT_COMPLETE)]
        public double MetadataPercentComplete { get; set; }

        [JsonProperty(TorrentFields.NAME)] public string Name { get; set; }

        [JsonProperty(TorrentFields.PEER_LIMIT)]
        public int PeerLimit { get; set; }

        [JsonProperty(TorrentFields.PEERS)] public TransmissionTorrentPeers[] Peers { get; set; }

        [JsonProperty(TorrentFields.PEERS_CONNECTED)]
        public int PeersConnected { get; set; }

        [JsonProperty(TorrentFields.PEERS_FROM)]
        public TransmissionTorrentPeersFrom PeersFrom { get; set; }

        [JsonProperty(TorrentFields.PEERS_SENDING_TO_US)]
        public int PeersSendingToUs { get; set; }

        [JsonProperty(TorrentFields.PERCENT_DONE)]
        public double PercentDone { get; set; }

        [JsonProperty(TorrentFields.PIECES)] public string Pieces { get; set; }

        [JsonProperty(TorrentFields.PIECE_COUNT)]
        public int PieceCount { get; set; }

        [JsonProperty(TorrentFields.PIECE_SIZE)]
        public int PieceSize { get; set; }

        [JsonProperty(TorrentFields.PRIORITIES)]
        public int[] Priorities { get; set; }

        [JsonProperty(TorrentFields.QUEUE_POSITION)]
        public int QueuePosition { get; set; }

        [JsonProperty(TorrentFields.RATE_DOWNLOAD)]
        public int RateDownload { get; set; }

        [JsonProperty(TorrentFields.RATE_UPLOAD)]
        public int RateUpload { get; set; }

        [JsonProperty(TorrentFields.RECHECK)] 
        public double RecheckProgress { get; set; }

        [JsonProperty(TorrentFields.SECONDS_DOWNLOADING)]
        public int SecondsDownloading { get; set; }

        [JsonProperty(TorrentFields.SECONDS_SEEDING)]
        public int SecondsSeeding { get; set; }

        [JsonProperty(TorrentFields.SEED_IDLE_LIMIT)]
        public int SeedIdleLimit { get; set; }

        [JsonProperty(TorrentFields.SEED_IDLE_MODE)]
        public int SeedIdleMode { get; set; }

        [JsonProperty(TorrentFields.SEED_RATIO_LIMIT)]
        public double SeedRatioLimit { get; set; }

        [JsonProperty(TorrentFields.SEED_RATIO_MODE)]
        public int SeedRatioMode { get; set; }

        [JsonProperty(TorrentFields.SIZE_WHEN_DONE)]
        public long SizeWhenDone { get; set; }

        [JsonProperty(TorrentFields.START_DATE)]
        public int StartDate { get; set; }

        /// <summary>
        /// <para>0: 'stopped'</para> 
        /// <para>1: 'check pending'</para> 
        /// <para>2: 'checking'</para> 
        /// <para>3: 'download pending'</para> 
        /// <para>4: 'downloading'</para> 
        /// <para>5: 'seed pending'</para> 
        /// <para>6: 'seeding' </para> 
        /// </summary>
        ///
        [JsonProperty(TorrentFields.STATUS)]
        public int Status { get; set; }

        [JsonProperty(TorrentFields.TRACKERS)] 
        public TransmissionTorrentTrackers[] Trackers { get; set; }

        [JsonProperty(TorrentFields.TRACKER_STATS)]
        public TransmissionTorrentTrackerStats[] TrackerStats { get; set; }

        [JsonProperty(TorrentFields.TOTAL_SIZE)]
        public long TotalSize { get; set; }

        [JsonProperty(TorrentFields.TORRENT_FILE)]
        public string TorrentFile { get; set; }

        [JsonProperty(TorrentFields.UPLOADED_EVER)]
        public long UploadedEver { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_LIMIT)]
        public long UploadLimit { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_LIMITED)]
        public bool UploadLimited { get; set; }

        [JsonProperty(TorrentFields.UPLOAD_RATIO)]
        public double UploadRatio { get; set; }

        [JsonProperty(TorrentFields.WANTED)] public bool[] Wanted { get; set; }

        [JsonProperty(TorrentFields.WEB_SEEDS)]
        public string[] Webseeds { get; set; }

        [JsonProperty(TorrentFields.WEB_SEEDS_SENDING_TO_US)]
        public int WebseedsSendingToUs { get; set; }

        public int CompareTo(object obj)
        {
            return Status.CompareTo(((TorrentInfo) obj).Status);
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public void Set(string propertyName, object value)
        {
            var myType = typeof(TorrentInfo);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);
        }

        public object Get(string propertyName)
        {
            var myType = typeof(TorrentInfo);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }


        public override bool Equals(object obj)
        {
            if (obj != null && obj is TorrentInfo torrentInfo)
                return torrentInfo.Id == Id &&
                       torrentInfo.HashString == HashString;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }


        #region parser

        public class TransmissionValue
        {
            public TransmissionValue(DataType dType, object dValue)
            {
                Type = dType;
                Value = dValue;
            }

            public object Value { get; }

            public DataType Type { get; }
        }

        public enum DataType
        {
            String,
            Int,
            List,
            Dictionary,
            Byte
        }

        private readonly BinaryReader _reader;
        private long _infoStart;
        private long _infoEnd;
        private long _totalValues;
        private readonly Dictionary<string, TransmissionValue> _root;

        [JsonIgnore]
        public Dictionary<string, TransmissionValue> Info =>
            _root["info"].Value as Dictionary<string, TransmissionValue>;

        public static bool TryParse(byte[] bytes, out TorrentInfo torrentParser)
        {
            try
            {
                torrentParser = new TorrentInfo(bytes);
                return true;
            }
            catch (Exception)
            {
                torrentParser = null;
                return false;
            }
        }

        public TorrentInfo(byte[] data)
        {
            using (_reader = new BinaryReader(new MemoryStream(data), Encoding.UTF8))
            {
                if (_reader.ReadChar() != 'd') throw new Exception("Torrent File Error");
                _root = parseDict();
                //var ShaHash = GetShaHash();
                Files = GetFiles();
                Name = Info.FindText("name");
                Trackers = GetTrackers();
            }
        }

        private Dictionary<string, TransmissionValue> parseDict()
        {
            var strs = new Dictionary<string, TransmissionValue>();
            while (_reader.PeekChar() != 101)
            {
                var str = parseString();
                if (str == "info") _infoStart = _reader.BaseStream.Position;
                if (str != "pieces")
                {
                    var tVal = ProcessVal();
                    strs.Add(str, tVal);
                }
                else
                {
                    strs.Add(str, parseByte());
                }

                if (str != "info") continue;
                _infoEnd = _reader.BaseStream.Position - _infoStart;
            }

            _reader.Read();
            return strs;
        }


        private TransmissionValue parseByte()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_reader.Read()));
            } while (char.ConvertFromUtf32(_reader.PeekChar()) != ":");

            _reader.Read();
            return new TransmissionValue(DataType.Byte, _reader.ReadBytes(int.Parse(str.ToString())));
        }


        private TransmissionValue ProcessVal()
        {
            _totalValues++;
            var str = char.ConvertFromUtf32(_reader.PeekChar());
            if (str == "d")
            {
                _reader.Read();
                return new TransmissionValue(DataType.Dictionary, parseDict());
            }

            if (str == "l")
            {
                _reader.Read();
                return new TransmissionValue(DataType.List, parseList());
            }

            if (str == "i")
            {
                _reader.Read();
                return new TransmissionValue(DataType.Int, parseNum());
            }

            return new TransmissionValue(DataType.String, parseString());
        }


        private List<TransmissionValue> parseList()
        {
            var tVals = new List<TransmissionValue>();
            while (char.ConvertFromUtf32(_reader.PeekChar()) != "e") tVals.Add(ProcessVal());
            _reader.Read();
            return tVals;
        }

        private long parseNum()
        {
            var str = new StringBuilder();
            do
            {
                str = str.Append(char.ConvertFromUtf32(_reader.Read()));
            } while (char.ConvertFromUtf32(_reader.PeekChar()) != "e");

            _reader.Read();
            return long.Parse(str.ToString());
        }

        private string parseString()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_reader.Read()));
            } while (char.ConvertFromUtf32(_reader.PeekChar()) != ":");

            _reader.Read();
            var numArray = _reader.ReadBytes(int.Parse(str.ToString()));
            return Encoding.UTF8.GetString(numArray);
        }

        private TransmissionTorrentFiles[] GetFiles()
        {
            var tFiles = new List<TransmissionTorrentFiles>();
            if (!Info.ContainsKey("files"))
            {
                tFiles.Add(new TransmissionTorrentFiles((long) Info["length"].Value, (string) Info["name"].Value));
                TotalSize = (long) Info["length"].Value;
            }
            else
            {
                long citem = 0;
                var v = (List<TransmissionValue>) Info["files"].Value;
                foreach (var tVal in v)
                {
                    var PieceLengthval = Info.FindNumber("piece length");
                    var strs = (Dictionary<string, TransmissionValue>) tVal.Value;
                    var pieceLength = (int) (citem / PieceLengthval) + 1;
                    citem = citem + (long) strs["length"].Value;
                    var num = (int) (citem / PieceLengthval) + 2 - pieceLength;
                    tFiles.Add(new TransmissionTorrentFiles((long) strs["length"].Value,
                        ((List<TransmissionValue>) strs["path"].Value).Select(c => c.Value as string).ToArray()));
                }

                TotalSize = citem;
            }

            return tFiles.Where(c => !c.Name.StartsWith("_____padding_file")).ToArray();
        }
        
        private TransmissionTorrentTrackers[] GetTrackers()
        {
            if (!_root.ContainsKey("announce-list"))
            {
                return new TransmissionTorrentTrackers[0];
            }
            var strs = new List<string>();
            foreach (var item in (List<TransmissionValue>)_root["announce-list"].Value)
            {
                foreach (var tVal in (List<TransmissionValue>)item.Value)
                {
                    var str = (string)tVal.Value;
                    if (strs.Contains(str))
                    {
                        continue;
                    }
                    strs.Add(str);
                }
            }
            return strs.Select(tr => new TransmissionTorrentTrackers() {announce = tr}).ToArray();
        }
        #endregion
    }

    internal static class TorrentExt
    {
        internal static T Find<T>(this Dictionary<string, TorrentInfo.TransmissionValue> dictToSearch, string key)
        {
            if (!dictToSearch.ContainsKey(key)) return default;
            return (T) dictToSearch[key].Value;
        }

        internal static long FindNumber(this Dictionary<string, TorrentInfo.TransmissionValue> dictToSearch, string key)
        {
            return dictToSearch.Find<long>(key);
        }

        internal static string FindText(this Dictionary<string, TorrentInfo.TransmissionValue> dictToSearch, string key)
        {
            return dictToSearch.Find<string>(key);
        }
    }

    //TODO: Separate "remove" and "active" torrents in "torrentsGet"
}