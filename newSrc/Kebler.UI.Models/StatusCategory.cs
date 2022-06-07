using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Kebler.Domain.Models;

namespace Kebler.UI.Models
{
    [DebuggerDisplay("{Title} ({Count})")]
    public class StatusCategory : PropertyChangedBase
    {
        private string _count = "~";
        public string Title { get; set; } = "~";
        public Categories Cat { get; set; }

        public string Count
        {
            get => _count;
            set
            {
                var dd = Set(ref _count, value);
            }
        }
    }
}
