using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.SI
{
    public interface ISingle
    {
        public void OnInstanceInvoked(string[] args);
    }
}
