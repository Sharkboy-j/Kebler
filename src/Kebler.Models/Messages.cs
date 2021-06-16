using System.Collections.Generic;
using System.Globalization;

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
            public ConnectedServerChanged(Server? server)
            {
                Server = server;
            }
            
            public readonly Server? Server;
        }

        public class ReconnectRequested
        {
            public ReconnectRequested(Server server)
            {
                Server = server;
            }

            public Server Server;
        }

        public class ReconnectAllowed
        {
        }

        public class ShowMoreInfoChanged
        {
        }

        public class DownlaodCategoriesChanged
        {
            public DownlaodCategoriesChanged(IEnumerable<FolderCategory> cats)
            {
                Paths = cats;
            }

            public IEnumerable<FolderCategory> Paths;
        }
    }
}