using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Text;
using Kebler.Models.Torrent;
using Kebler.Resources;

namespace Kebler.Services
{
    public static class Utils
    {
 
        public static bool IsNullOrEmpty(this IEnumerable current)
        {
            return current is null || !current.GetEnumerator().MoveNext();
        }

        public static string GetSizeString(long length, bool showEmpty = false)
        {
            long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double size = length;
            var suffix = nameof(B);

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
            else if (length >= B)
            {
                return $"{size} {nameof(B)}";
            }
            else
            {
                return showEmpty ? $"0 {nameof(KB)}" : string.Empty;
            }

            size = Math.Round(length / SelSize, 2);

            return $"{size} {suffix}";
        }

        public static string ToPrettyFormat(this TimeSpan span)
        {
            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.Append($"{span.Days}{LocalizationProvider.GetLocalizedValue(nameof(Strings.time_days_short))} ");
            if (span.Hours > 0)
                sb.Append($"{span.Hours}{LocalizationProvider.GetLocalizedValue(nameof(Strings.time_hours_short))} ");
            if (span.Minutes > 0)
                sb.Append($"{span.Minutes}{LocalizationProvider.GetLocalizedValue(nameof(Strings.time_minutes_short))} ");
            if (span.Seconds > 0)
                sb.Append($"{span.Seconds}{LocalizationProvider.GetLocalizedValue(nameof(Strings.time_seconds_short))} ");
            return sb.ToString();
        }


        public static string GetErrorString(TorrentInfo ti)
        {
            if (string.IsNullOrEmpty(ti.ErrorString))
            {
                var lastAnnounceSucceeded = ti.TrackerStats.All(x => x.LastAnnounceSucceeded == false);
                if (lastAnnounceSucceeded)
                {
                    var txtError = ti.TrackerStats.FirstOrDefault(x => !string.IsNullOrEmpty(x.LastAnnounceResult))
                        ?.LastAnnounceResult;
                    return txtError;
                }

                return string.Empty;
            }

            return ti.ErrorString;
        }

        public static Bitmap CreatePiecesBitmap(int pieceCount, string _piecesBase64, double height)
        {
            var decodedPices = _piecesBase64.Length > 0 ? Convert.FromBase64CharArray(_piecesBase64.ToCharArray(), 0, _piecesBase64.Length) : new byte[0];

            var bmp = new Bitmap(decodedPices.Length, (int)height);

            try
            {
                using Graphics g = Graphics.FromImage(bmp);
                var c_bit = 0;
                var bitsperrow = bmp.Width > 0 ? pieceCount / (float)bmp.Width : 0;

                if (bitsperrow > 0)
                {
                    for (var n = 0; n < bmp.Width; n++)
                    {
                        var num_bits = (int)(bitsperrow * (n + 1)) - c_bit;
                        var bits_got = 0;
                        for (var i = 0; i < num_bits; i++)
                        {
                            if (BitGet(decodedPices, pieceCount, c_bit + i))
                                bits_got++;
                        }

                        float chunk_done;
                        if (num_bits > 0)
                            chunk_done = (float)bits_got / (float)num_bits;
                        else if (BitGet(decodedPices, pieceCount, c_bit))
                            chunk_done = 1;
                        else
                            chunk_done = 0;

                        Color fill;

                        if (chunk_done == 1)
                            fill = Color.FromArgb(0, 122, 204);
                        else
                            fill = Color.FromArgb(50, 50, 50);

                        g.DrawLine(new Pen(fill), n, 0, n, bmp.Height);

                        c_bit += num_bits;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return bmp;

        }
        
        private static bool BitGet(byte[] array, int len, int index)
        {
            if (index < 0 || index >= len)
                throw new ArgumentOutOfRangeException();
            try
            {
                return (array[index >> 3] & (1 << ((7 - index) & 7))) != 0;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}