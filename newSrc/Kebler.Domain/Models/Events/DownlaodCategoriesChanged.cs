using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.Domain.Models.Events
{
    public class DownlaodCategoriesChanged
    {
        public DownlaodCategoriesChanged(IEnumerable<IFolderCategory> cats)
        {
            Paths = cats;
        }

        public IEnumerable<IFolderCategory> Paths;
    }
}
