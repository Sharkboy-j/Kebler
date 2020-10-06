using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Kebler.Models.Torrent;

namespace Kebler.Services
{
    public class TorrentParser
    {
        private readonly Dictionary<string, TransmissionValue> _root;
        private readonly BinaryReader _torrent;
        private long _infoEnd;
        private long _infoStart;

        public TorrentParser(byte[] data)
        {
            _torrent = new BinaryReader(new MemoryStream(data), Encoding.UTF8);
            if (_torrent.ReadChar() != 'd') throw new Exception("Torrent File Error");
            _root = ProcessDict();
            ShaHash = GetShaHash();
            Files = GetFiles();
            _torrent.Close();
        }

        public long TotalValues { get; private set; }

        public string MagnetUri
        {
            get
            {
                var sb = new StringBuilder($"magnet:?xt=urn:btih:{ShaHash}");
                if (!string.IsNullOrWhiteSpace(Name)) sb.Append($"&dn={WebUtility.UrlEncode(Name)}");
                if (AnnounceList.Length > 0)
                    sb.Append($"&tr={string.Join("&tr=", AnnounceList.Select(WebUtility.UrlEncode))}");
                return sb.ToString();
            }
        }

        public string ShaHash { get; }
        public byte[] ByteHash { get; private set; }
        public TransmissionTorrentFiles[] Files { get; }
        public long Size { get; private set; }
        public long PieceLength => Info.FindNumber("piece length");

        public Dictionary<string, TransmissionValue> Info =>
            _root["info"].Value as Dictionary<string, TransmissionValue>;

        public bool IsSingle => !Info.ContainsKey("files");
        public byte[] Pieces => Info["pieces"].Value as byte[];

        public string[] AnnounceList
        {
            get
            {
                if (!_root.ContainsKey("announce-list")) return new string[0];
                var strs = new List<string>();
                foreach (var item in (List<TransmissionValue>) _root["announce-list"].Value)
                foreach (var tVal in (List<TransmissionValue>) item.Value)
                {
                    var str = (string) tVal.Value;
                    if (strs.Contains(str)) continue;
                    strs.Add(str);
                }

                return strs.ToArray();
            }
        }

        public string AnnounceUrl => _root.FindText("announce");
        public string Comment => _root.FindText("comment");
        public string CreatedBy => _root.FindText("created by");
        public string Name => Info.FindText("name");
        public bool Private => Info.FindText("private") == "1";

        public DateTime CreationDate
        {
            get
            {
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
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

        private TransmissionTorrentFiles[] GetFiles()
        {
            var tFiles = new List<TransmissionTorrentFiles>();
            if (!Info.ContainsKey("files"))
            {
                tFiles.Add(new TransmissionTorrentFiles((long) Info["length"].Value, (string) Info["name"].Value));
                Size = (long) Info["length"].Value;
            }
            else
            {
                long citem = 0;
                var v = (List<TransmissionValue>) Info["files"].Value;
                foreach (var tVal in v)
                {
                    var strs = (Dictionary<string, TransmissionValue>) tVal.Value;
                    var pieceLength = (int) (citem / PieceLength) + 1;
                    citem = citem + (long) strs["length"].Value;
                    var num = (int) (citem / PieceLength) + 2 - pieceLength;
                    tFiles.Add(new TransmissionTorrentFiles((long) strs["length"].Value,
                        ((List<TransmissionValue>) strs["path"].Value).Select(c => c.Value as string).ToArray()));
                }

                Size = citem;
            }

            return tFiles.Where(c => !c.Name.StartsWith("_____padding_file")).ToArray();
        }

        private string GetShaHash()
        {
            var sHa1Managed = new SHA1Managed();
            _torrent.BaseStream.Position = _infoStart;
            var numArray = _torrent.ReadBytes(Convert.ToInt32(_infoEnd));
            ByteHash = sHa1Managed.ComputeHash(numArray);
            return BitConverter.ToString(ByteHash).Replace("-", "");
        }

        private TransmissionValue ProcessVal()
        {
            TotalValues++;
            var str = char.ConvertFromUtf32(_torrent.PeekChar());
            if (str == "d")
            {
                _torrent.Read();
                return new TransmissionValue(DataType.Dictionary, ProcessDict());
            }

            if (str == "l")
            {
                _torrent.Read();
                return new TransmissionValue(DataType.List, ProcessList());
            }

            if (str == "i")
            {
                _torrent.Read();
                return new TransmissionValue(DataType.Int, ProcessInt());
            }

            return new TransmissionValue(DataType.String, ProcessString());
        }

        private string ProcessString()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_torrent.Read()));
            } while (char.ConvertFromUtf32(_torrent.PeekChar()) != ":");

            _torrent.Read();
            var numArray = _torrent.ReadBytes(int.Parse(str.ToString()));
            return Encoding.UTF8.GetString(numArray);
        }

        private List<TransmissionValue> ProcessList()
        {
            var tVals = new List<TransmissionValue>();
            while (char.ConvertFromUtf32(_torrent.PeekChar()) != "e") tVals.Add(ProcessVal());
            _torrent.Read();
            return tVals;
        }

        private long ProcessInt()
        {
            var str = new StringBuilder();
            do
            {
                str = str.Append(char.ConvertFromUtf32(_torrent.Read()));
            } while (char.ConvertFromUtf32(_torrent.PeekChar()) != "e");

            _torrent.Read();
            return long.Parse(str.ToString());
        }

        private Dictionary<string, TransmissionValue> ProcessDict()
        {
            var strs = new Dictionary<string, TransmissionValue>();
            while (_torrent.PeekChar() != 101)
            {
                var str = ProcessString();
                if (str == "info") _infoStart = _torrent.BaseStream.Position;
                if (str != "pieces")
                {
                    var tVal = ProcessVal();
                    strs.Add(str, tVal);
                }
                else
                {
                    strs.Add(str, ProcessByte());
                }

                if (str != "info") continue;
                _infoEnd = _torrent.BaseStream.Position - _infoStart;
            }

            _torrent.Read();
            return strs;
        }

        private TransmissionValue ProcessByte()
        {
            var str = new StringBuilder();
            do
            {
                str.Append(char.ConvertFromUtf32(_torrent.Read()));
            } while (char.ConvertFromUtf32(_torrent.PeekChar()) != ":");

            _torrent.Read();
            return new TransmissionValue(DataType.Byte, _torrent.ReadBytes(int.Parse(str.ToString())));
        }

        #endregion
    }

    #region Types

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

    #endregion
}