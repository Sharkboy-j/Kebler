using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Domain.Interfaces
{
    public interface ICustomLocalizationProvider
    {
        public string GetLocalizedValue(string key);
    }
}
