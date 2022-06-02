using System;
using System.Diagnostics;
using Kebler.Domain.Interfaces;
using LiteDB;
using Newtonsoft.Json;

namespace Kebler.Domain.Models
{

    [DebuggerDisplay("{Host}")]
    public class Server : IServer
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;

        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool AuthEnabled { get; set; } = false;

        public bool AskForPassword { get; set; } = false;

        public bool SslEnabled { get; set; }

        public ClientType ClientType { get; set; }

        public string RpcPath { get; set; } = @"/transmission/rpc";

        public bool Equals(IServer other)
        {
            if (other is Server srv)
                return srv.Id == Id;

            return false;
        }

        [BsonIgnore]
        [JsonIgnore]
        public string FullUriPath
        {
            get
            {
                if (string.IsNullOrEmpty(Host))
                    return string.Empty;

                var scheme = SslEnabled ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

                UriBuilder builder = new();

                switch (ClientType)
                {
                    case ClientType.Transmission:
                        builder = new UriBuilder(scheme, Host, Port, RpcPath);
                        break;
                    case ClientType.Qbittorrent:
                        builder = new UriBuilder(scheme, Host, Port);
                        break;
                }
                
                return builder.Uri.AbsoluteUri;
            }
        }
    }
}