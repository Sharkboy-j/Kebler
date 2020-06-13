namespace Transmission.API.RPC.Entity
{
    public class Enums
    {
        public enum AddTorrentStatus { Added, Duplicate, UnknownError, ResponseNull}
        public enum RemoveResult { Ok, Error}
        public enum ErrorsResponse { TimeOut, HttpStatusCodeConflict, IsNotHttpWebResponse, None,SessionIdError}
        public enum ReponseResult { Ok, NotOk }
    }
}
