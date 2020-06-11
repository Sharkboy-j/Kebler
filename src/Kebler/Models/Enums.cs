namespace Kebler.Models
{
    public class Enums
    {
        public enum ValidateError { Ok, TitileEmpty, TitileExists, IpOrHostError, PortError, RpcPathEmpty }
        public enum FilterType { Name, Path }
        public enum MessageBoxDilogButtons { YesNo, OkCancel, Ok, None }
        public enum Categories : byte
        {
            All, Downloading, Active, Stopped, Ended, Error, Inactive
        }
    }
}
