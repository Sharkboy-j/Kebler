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

        public enum AddTorrentStatus { Added, Duplicate, UnknownError, ResponseNull }
        public enum RemoveResult { Ok, Error }
        public enum ErrorsResponse { TimeOut, HttpStatusCodeConflict, IsNotHttpWebResponse, None, SessionIdError, WebException }
        public enum ReponseResult { Ok, NotOk }
    }
}
