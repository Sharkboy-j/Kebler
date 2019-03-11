namespace Kebler.Models
{
    public class Server
    {
        public uint Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool AuthEnabled { get; set; } = false;
        public bool AskForPassword { get; set; } = false;

        public string RPCPath { get; set; } = @"/transmission/rpc";
    }
}
