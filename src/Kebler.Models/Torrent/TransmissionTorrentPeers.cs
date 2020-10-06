using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Newtonsoft.Json;

namespace Kebler.Models.Torrent
{
    public class TransmissionTorrentPeers : IEqualityComparer, IEqualityComparer<TransmissionTorrentPeers>
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("clientIsChoked")]
        public bool ClientIsChoked { get; set; }

        [JsonProperty("clientIsInterested")]
        public bool ClientIsInterested { get; set; }

        [JsonProperty("flagStr")]
        public string FlagStr { get; set; }

        [JsonProperty("isDownloadingFrom")]
        public bool IsDownloadingFrom { get; set; }

        [JsonProperty("isEncrypted")]
        public bool IsEncrypted { get; set; }

        [JsonProperty("isUploadingTo")]
        public bool IsUploadingTo { get; set; }

        [JsonProperty("isUTP")]
        public bool IsUTP { get; set; }

        [JsonProperty("peerIsChoked")]
        public bool PeerIsChoked { get; set; }

        [JsonProperty("peerIsInterested")]
        public bool PeerIsInterested { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("progress")]
        public double Progress { get; set; }

        [JsonProperty("rateToClient")]
        public int RateToClient { get; set; }

        [JsonProperty("rateToPeer")]
        public int RateToPeer { get; set; }

        public new bool Equals(object x, object y)
        {
            if (x is TransmissionTorrentPeers _x && y is TransmissionTorrentPeers _y)
                return Equals(_x, _y);
            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj is TransmissionTorrentPeers _o)
                return GetHashCode(_o);
            return base.GetHashCode();
        }

        public bool Equals([AllowNull] TransmissionTorrentPeers x, [AllowNull] TransmissionTorrentPeers y)
        {
            return x.Address.Equals(y.Address)
                   && x.ClientName.Equals(y.ClientName)
                   && x.Port.Equals(y.Port);
        }

        public int GetHashCode([DisallowNull] TransmissionTorrentPeers obj)
        {
            return $"{obj.Address}{obj.Port}{obj.ClientName}".GetHashCode();
        }

        public void Set(string propertyName, object value)
        {
            var myType = typeof(TransmissionTorrentPeers);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);
        }

        public object Get(string propertyName)
        {
            var myType = typeof(TransmissionTorrentPeers);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }


        public void Update(TransmissionTorrentPeers peer)
        {
            foreach (var propertyInfo in GetType().GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance))
                Set(propertyInfo.Name, peer.Get(propertyInfo.Name));
        }
    }
}