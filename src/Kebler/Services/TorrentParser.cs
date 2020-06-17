using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kebler.Services
{

    public class TorrentParser
    {


        public TorrentParser(byte[] data)
        {
            _torrent = new BinaryReader(new MemoryStream(data), Encoding.UTF8);
            if (_torrent.ReadChar() != 'd')
            {
                throw new Exception("Torrent File Error");
            }
            _root = ProcessDict();
            SHAHash = GetSHAHash();
            Files = GetFiles();
            _torrent.Close();
        }



        private readonly Dictionary<string, TValue> _root;
        private readonly BinaryReader _torrent;
        private long _infoStart;
        private long _infoEnd;

        public long TotalValues { get; private set; }

        public string MagnetURI
        {
            get
            {
                var sb = new StringBuilder($"magnet:?xt=urn:btih:{SHAHash}");
                if (!string.IsNullOrWhiteSpace(Name)) sb.Append($"&dn={System.Net.WebUtility.UrlEncode(Name)}");
                if (AnnounceList.Length > 0) sb.Append($"&tr={string.Join("&tr=", AnnounceList.Select(System.Net.WebUtility.UrlEncode))}");
                return sb.ToString();
            }
        }

        public string SHAHash { get; }
        public byte[] ByteHash { get; private set; }
        public TFile[] Files { get; private set; }
        public long Size { get; private set; }
        public long PieceLength => Info.FindNumber("piece length");
        public Dictionary<string, TValue> Info => _root["info"].Value as Dictionary<string, TValue>;
        public bool IsSingle => !Info.ContainsKey("files");
        public byte[] Pieces => Info["pieces"].Value as byte[];
        public string[] AnnounceList
        {
            get
            {
                if (!_root.ContainsKey("announce-list"))
                {
                    return new string[0];
                }
                var strs = new List<string>();
                foreach (var item in (List<TValue>)_root["announce-list"].Value)
                {
                    foreach (var tVal in (List<TValue>)item.Value)
                    {
                        var str = (string)tVal.Value;
                        if (strs.Contains(str))
                        {
                            continue;
                        }
                        strs.Add(str);
                    }
                }
                return strs.ToArray();
            }
        }
        public string AnnounceURL => _root.FindText("announce");
        public string Comment => _root.FindText("comment");
        public string CreatedBy => _root.FindText("created by");
        public string Name => Info.FindText("name");
        public bool Private => Info.FindText("private") == "1";

        public DateTime CreationDate
        {
            get
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(_root.FindNumber("creation date"));
                return dateTime;
            }
        }

        public string FileEncoding => Info.FindText("encoding");


        public static bool TryParse(string path, out TorrentParser torrentParser)
        {
            try
            {
                torrentParser = new TorrentParser(File.ReadAllBytes(path));
                return true;
            }
            catch (Exception)
            {
                torrentParser = null;
                return false;
            }
        }

        public static bool TryParse(byte[] bytes, out TorrentParser torrentParser)
        {
            try
            {
                torrentParser = new TorrentParser(bytes);
                return true;
            }
            catch (Exception)
            {
                torrentParser = null;
                return false;
            }
        }
        public static TorrentParser Parse(string path)
        {
            return new TorrentParser(File.ReadAllBytes(path));
        }

        #region PRC
        private TFile[] GetFiles()
        {
            var tFiles = new List<TFile>();
            if (!Info.ContainsKey("files"))
            {
                tFiles.Add(new TFile((long)Info["length"].Value, 1, Pieces.Length / 20, (string)Info["name"].Value));
                Size = (long)Info["length"].Value;
            }
            else
            {
                long citem = 0;
                var v = (List<TValue>)Info["files"].Value;
                foreach (var tVal in v)
                {
                    var strs = (Dictionary<string, TValue>)tVal.Value;
                    var pieceLength = (int)(citem / PieceLength) + 1;
                    citem = citem + (long)strs["length"].Value;
                    var num = (int)(citem / PieceLength) + 2 - pieceLength;
                    tFiles.Add(new TFile((long)strs["length"].Value, pieceLength, num, ((List<TValue>)strs["path"].Value).Select(c => c.Value as string).ToArray()));
                }
                Size = citem;
            }
            return tFiles.Where(c => !c.Name.StartsWith("_____padding_file")).ToArray();
        }
        private string GetSHAHash()
        {
            var sHA1Managed = new SHA1Managed();
            _torrent.BaseStream.Position = _infoStart;
            var numArray = _torrent.ReadBytes(Convert.ToInt32(_infoEnd));
            ByteHash = sHA1Managed.ComputeHash(numArray);
            return BitConverter.ToString(ByteHash).Replace("-", "");
        }
        private TValue ProcessVal()
        {
            TotalValues++;
            var str = char.ConvertFromUtf32(_torrent.PeekChar());
            if (str == "d")
            {
                _torrent.Read();
                return new TValue(DataType.Dictionary, ProcessDict());
            }
            if (str == "l")
            {
                _torrent.Read();
                return new TValue(DataType.List, ProcessList());
            }
            if (str == "i")
            {
                _torrent.Read();
                return new TValue(DataType.Int, ProcessInt());
            }
            return new TValue(DataType.String, ProcessString());
        }

        private string ProcessString()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_torrent.Read()));
            }
            while (char.ConvertFromUtf32(_torrent.PeekChar()) != ":");
            _torrent.Read();
            var numArray = _torrent.ReadBytes(int.Parse(str.ToString()));
            return Encoding.UTF8.GetString(numArray);
        }

        private List<TValue> ProcessList()
        {
            var tVals = new List<TValue>();
            while (char.ConvertFromUtf32(_torrent.PeekChar()) != "e")
            {
                tVals.Add(ProcessVal());
            }
            _torrent.Read();
            return tVals;
        }

        private long ProcessInt()
        {
            var str = new StringBuilder();
            do
            {
                str = str.Append(char.ConvertFromUtf32(_torrent.Read()));
            }
            while (char.ConvertFromUtf32(_torrent.PeekChar()) != "e");
            _torrent.Read();
            return long.Parse(str.ToString());
        }

        private Dictionary<string, TValue> ProcessDict()
        {
            var strs = new Dictionary<string, TValue>();
            while (_torrent.PeekChar() != 101)
            {
                string str = ProcessString();
                if (str == "info")
                {
                    _infoStart = _torrent.BaseStream.Position;
                }
                if (str != "pieces")
                {
                    TValue tVal = ProcessVal();
                    strs.Add(str, tVal);
                }
                else
                {
                    strs.Add(str, ProcessByte());
                }
                if (str != "info")
                {
                    continue;
                }
                _infoEnd = _torrent.BaseStream.Position - _infoStart;
            }
            _torrent.Read();
            return strs;
        }

        private TValue ProcessByte()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_torrent.Read()));
            } while (char.ConvertFromUtf32(_torrent.PeekChar()) != ":");
            _torrent.Read();
            return new TValue(DataType.Byte, _torrent.ReadBytes(int.Parse(str.ToString())));
        }
        #endregion
    }

    internal static class TorrentExt
    {
        internal static T Find<T>(this Dictionary<string, TValue> dictToSearch, string key)
        {
            if (!dictToSearch.ContainsKey(key))
            {
                return default(T);
            }
            return (T)dictToSearch[key].Value;
        }

        internal static long FindNumber(this Dictionary<string, TValue> dictToSearch, string key)
        {
            return dictToSearch.Find<long>(key);
        }

        internal static string FindText(this Dictionary<string, TValue> dictToSearch, string key)
        {
            return dictToSearch.Find<string>(key);
        }
    }

    #region Types
    public class TFile
    {
        public int FirstPiece { get; }

        public long Length { get; }

        public string Name { get; }

        public string Path { get; }

        public long PieceLength { get; }


        public TFile(long size, int pStart, int pLen, params string[] fullPath)
        {
            Length = size < 0 ? 0 : size;
            FirstPiece = pStart;
            PieceLength = pLen;
            Path = string.Join("\\", fullPath);
            var num = Path.LastIndexOf("\\", StringComparison.Ordinal);
            if (num <= 0)
            {
                Name = Path;
                Path = "";
                return;
            }
            Name = Path.Substring(num + 1);
            Path = Path.Substring(0, num);
        }
    }

    public class TValue
    {
        public object Value { get; }

        public DataType Type { get; }

        public TValue(DataType dType, object dValue)
        {
            Type = dType;
            Value = dValue;
        }
    }

    public enum DataType
    {
        String,
        Int,
        List,
        Dictionary,
        Byte
    }
    #endregion
}
