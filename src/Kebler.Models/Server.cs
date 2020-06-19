using LiteDB;
using Newtonsoft.Json;

namespace Kebler.Models
{
    public class Server
    {
        public Server()
        {
        }
        public Server(int id)
        {
            Id = id;
        }

        public int Id { get; }
        public string Title { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        public bool AuthEnabled { get; set; } = false;
        public bool AskForPassword { get; set; } = false;
        public bool SslEnabled { get; set; } = true;

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
                var type = SslEnabled ? "https://" : "http://";

                if (!RpcPath.StartsWith("/")) RpcPath = $"/{RpcPath}";
                var uri = $"{type}{Host}:{Port}{RpcPath}";
                if (!uri.EndsWith("/")) uri += "/";
                return uri;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is Server srv)
            {
                return srv.Id == Id;
            }
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
    }
}
