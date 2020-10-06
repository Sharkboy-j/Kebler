using System.Collections.Generic;

namespace Kebler.Services
{
    internal static class TorrentExt
    {
        internal static T Find<T>(this Dictionary<string, TransmissionValue> dictToSearch, string key)
        {
            if (!dictToSearch.ContainsKey(key)) return default;
            return (T) dictToSearch[key].Value;
        }

        internal static long FindNumber(this Dictionary<string, TransmissionValue> dictToSearch, string key)
        {
            return dictToSearch.Find<long>(key);
        }

        internal static string FindText(this Dictionary<string, TransmissionValue> dictToSearch, string key)
        {
            return dictToSearch.Find<string>(key);
        }
    }
}