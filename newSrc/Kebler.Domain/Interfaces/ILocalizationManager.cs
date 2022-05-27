using System.Collections.Generic;
using System.Globalization;

namespace Kebler.Domain.Interfaces
{
    public interface ILocalizationManager
    {
        public List<CultureInfo> CultureList { get; }

        public CultureInfo CurrentCulture { get; set; }
    }
}