using System;
using System.Collections.Generic;
using System.Text;

namespace Transmission.API.RPC.Entity
{
    public class Enums
    {
        public enum AddResult { Added, Duplicate, Error, ResponseNull};
        public enum RemoveResult { Ok, Error};
        public enum ErrorsResponse { TimeOut, HttpStatusCodeConflict, IsNotHttpWebResponse, Ok };
    }
}
