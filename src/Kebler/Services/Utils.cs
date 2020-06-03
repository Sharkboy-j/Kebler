using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kebler.Models;

namespace Kebler.Services
{
    public static class Utils
    {

        public static void LogServers<T>(this List<T> serverList)
        {
            if (serverList.Count == 0)
            {
                App.Log.Info("ServerListCount: 0");
            }
            var txt = $"ServersList[{serverList.Count}]:{Environment.NewLine}";
            foreach (var server in serverList)
            {
                txt += server + Environment.NewLine;
            }
            App.Log.Info(txt);
        }

        public static string GetSizeString(long length)
        {
            long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double size = length;
            string suffix = nameof(B);

            double SelSize = 0;

            if (length >= TB)
            {
                SelSize = TB;
                suffix = nameof(TB);
            }
            else if (length >= GB)
            {
                SelSize = GB;
                suffix = nameof(GB);
            }
            else if (length >= MB)
            {
                SelSize = MB;
                suffix = nameof(MB);
            }
            else if (length >= KB)
            {
                SelSize = KB;
                suffix = nameof(KB);
            }
            else
            {
                return null;// $"0 bt/s";
            }

            size = Math.Round(length / SelSize, 2);

            return $"{size} {suffix}";
        }

        public static string ToPrettyFormat(this TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0} d ", span.Days);
            if (span.Hours > 0)
                sb.AppendFormat("{0} h ", span.Hours);
            if (span.Minutes > 0)
                sb.AppendFormat("{0} m ", span.Minutes);
            if (span.Seconds > 0)
                sb.AppendFormat("{0} s", span.Seconds);
            return sb.ToString();

        }
    }
}
