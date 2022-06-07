using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
    public class TorrentPeers : PropertyChangedBase, IEqualityComparer, IEqualityComparer<ITorrentPeers>, ITorrentPeers
    {
        private string _address;
        private string _clientName;
        private bool _clientIsChoked;
        private bool _clientIsInterested;
        private string _flagStr;
        private bool _isDownloadingFrom;
        private bool _isEncrypted;
        private bool _isUploadingTo;
        private bool _isUtp;
        private bool _peerIsChoked;
        private bool _peerIsInterested;
        private int _port;
        private double _progress;
        private int _rateToClient;
        private int _rateToPeer;

        public string Address
        {
            get => _address;
            set => Set(ref _address, value);
        }

        public string ClientName
        {
            get => _clientName;
            set => Set(ref _clientName, value);
        }

        public bool ClientIsChoked
        {
            get => _clientIsChoked;
            set => Set(ref _clientIsChoked, value);
        }

        public bool ClientIsInterested
        {
            get => _clientIsInterested;
            set => Set(ref _clientIsInterested, value);
        }

        public string FlagStr
        {
            get => _flagStr;
            set => Set(ref _flagStr, value);
        }

        public bool IsDownloadingFrom
        {
            get => _isDownloadingFrom;
            set => Set(ref _isDownloadingFrom, value);
        }

        public bool IsEncrypted
        {
            get => _isEncrypted;
            set => Set(ref _isEncrypted, value);
        }

        public bool IsUploadingTo
        {
            get => _isUploadingTo;
            set => Set(ref _isUploadingTo, value);
        }

        public bool IsUTP
        {
            get => _isUtp;
            set => Set(ref _isUtp, value);
        }

        public bool PeerIsChoked
        {
            get => _peerIsChoked;
            set => Set(ref _peerIsChoked, value);
        }

        public bool PeerIsInterested
        {
            get => _peerIsInterested;
            set => Set(ref _peerIsInterested, value);
        }

        public int Port
        {
            get => _port;
            set => Set(ref _port, value);
        }

        public double Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        public int RateToClient
        {
            get => _rateToClient;
            set => Set(ref _rateToClient, value);
        }

        public int RateToPeer
        {
            get => _rateToPeer;
            set => Set(ref _rateToPeer, value);
        }

        public new bool Equals(object x, object y)
        {
            if (x is TorrentPeers _x && y is TorrentPeers _y)
                return Equals(_x, _y);
            return false;
        }

        public int GetHashCode(object obj)
        {
            if (obj is TorrentPeers _o)
                return GetHashCode(_o);
            return base.GetHashCode();
        }

        public bool Equals([AllowNull] ITorrentPeers x, [AllowNull] ITorrentPeers y)
        {
            return x.Address.Equals(y.Address)
                   && x.ClientName.Equals(y.ClientName)
                   && x.Port.Equals(y.Port);
        }

        public int GetHashCode([DisallowNull] ITorrentPeers obj)
        {
            return $"{obj.Address}{obj.Port}{obj.ClientName}".GetHashCode();
        }

        public void Set(string propertyName, object value)
        {
            var myType = typeof(TorrentPeers);
            var myPropInfo = myType.GetProperty(propertyName);
            myPropInfo?.SetValue(this, value, null);
        }

        public object Get(string propertyName)
        {
            var myType = typeof(TorrentPeers);
            var myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo?.GetValue(this, null);
        }


        public void Update(TorrentPeers peer)
        {
            foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                Set(propertyInfo.Name, peer.Get(propertyInfo.Name));
        }
    }
}
