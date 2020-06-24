using System;
using Caliburn.Micro;
using LiteDB;
using Newtonsoft.Json;

namespace Kebler.Models
{
    public class Server: PropertyChangedBase
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        public bool AuthEnabled { get; set; } = false;
        public bool AskForPassword { get; set; } = false;

        private bool _sslEnabled = true;
        public bool SslEnabled
        {
            get => _sslEnabled;
            set => Set(ref _sslEnabled, value);
        }

        public string RpcPath { get; set; } = @"/transmission/rpc";

        [BsonIgnore]
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this,Formatting.Indented);
        }

        [BsonIgnore,JsonIgnore]
        public string FullUriPath
        {
            get
            {
                var scheme = SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

                var uri = new UriBuilder(scheme, Host, Port, RpcPath);

                //var type = SslEnabled ? "https://" : "http://";

                //if (!RpcPath.StartsWith("/")) RpcPath = $"/{RpcPath}";
                //var uri = $"{type}{Host}:{Port}{RpcPath}";
                //if (!uri.EndsWith("/")) uri += "/";
                return uri.Uri.AbsoluteUri;
            }
        }

        [BsonIgnore]
        public override bool Equals(object obj)
        {
            if (obj != null && obj is Server srv)
            {
                return srv.Id == Id;
            }
            return false;
        }

        [BsonIgnore]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }
    }
}
