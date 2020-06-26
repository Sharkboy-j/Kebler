using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kebler.Models
{
    public class Messages
    {
        public class LocalizationCultureChangesMessage
        {
            public CultureInfo Culture;
        }

        public class ServersUpdated
        {
        }

        public class ConnectedServerChanged
        {
            public Server srv;
        }

        public class ReconnectRequested
        {
            public Server srv;
        }

        public class ReconnectAllowed
        {
        }

    }
}
