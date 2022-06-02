using System;
using LiteDB;
using Newtonsoft.Json;

namespace Kebler.Domain.Interfaces
{
    public interface IServer : IEquatable<IServer>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Host { get; set; }

        public ushort Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool AuthEnabled { get; set; }

        public bool AskForPassword { get; set; }

        public bool SslEnabled { get; set; }

        public string RpcPath { get; set; }

        public ClientType ClientType { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public string FullUriPath { get; }
    }
}
