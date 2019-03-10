namespace Kebler.Models
{
    public class Server
    {

        public string Host { get; set; } = string.Empty;
        public ushort Port { get; set; } = 9091;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool AskForPassword { get; set; } = false;

        public string RPCPath { get; set; } = @"/transmission/rpc";
    }
}
