using LiteDB;
using Newtonsoft.Json;

namespace Kebler.Models
{
    public class Server
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        [JsonIgnore]
        public string Password { get; set; } = string.Empty;

        //public bool IsDefault { get; set; } = false;

        public bool AuthEnabled { get; set; } = false;
        public bool AskForPassword { get; set; } = false;
        public bool SSLEnabled { get; set; } = false;

        public string RPCPath { get; set; } = @"/transmission/rpc";

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
                var type = SSLEnabled ? "https://" : "http://";

                if (!RPCPath.StartsWith("/")) RPCPath = $"/{RPCPath}";
                var uri = $"{type}{Host}:{Port}{RPCPath}";
                if (!uri.EndsWith("/")) uri += "/";
                return uri;
            }
        }
    }
}
