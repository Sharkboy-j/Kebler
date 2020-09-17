using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Kebler.Models.Torrent;

namespace Kebler.Services
{
    public static class Utils
    {

        //public static void LogServers<T>(this List<T> serverList)
        //{
        //    if (serverList.Count == 0)
        //    {
        //        App.Log.Info("ServerListCount: 0");
        //    }
        //    var txt = $"ServersList[{serverList.Count}]:{Environment.NewLine}";
        //    foreach (var server in serverList)
        //    {
        //        txt += server + Environment.NewLine;
        //    }
        //    App.Log.Info(txt);
        //}

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
            else
            {
                return showEmpty ? $"0 KB" : string.Empty;
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


        public static string GetErrorString(TorrentInfo ti)
        {
            if (string.IsNullOrEmpty(ti.ErrorString))
            {
                var lastAnnounceSucceeded =  ti.TrackerStats.All(x => x.LastAnnounceSucceeded == false);
                if (lastAnnounceSucceeded)
                {
                    var txtError = ti.TrackerStats.FirstOrDefault(x => !string.IsNullOrEmpty(x.LastAnnounceResult))?.LastAnnounceResult;
                    return txtError;
                }

                return string.Empty;
            }
            else
            {
                return ti.ErrorString;
            }
        }

        public static Bitmap CreatePiecesBitmap(int pieceCount, string piecesString)
        {
            try
            {
                //int piecesDone = 0;

                if (pieceCount < 1)
                    return null;

                var pieces = Convert.FromBase64String(piecesString);


                var rowCount = 100;
                var result = new Bitmap(pieceCount, rowCount, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                var bitmapData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);
                var rowPixels = new byte[bitmapData.Stride];


                //yea yea yea, go further and call me idiot later @Shark
                void insertPixel(int index, byte r, byte g, byte b)
                {
                    rowPixels[index * 3] = b;
                    rowPixels[index * 3 + 1] = g;
                    rowPixels[index * 3 + 2] = r;
                }

                for (var i = 0; i < pieceCount; i++)
                {
                    // read bit at specific place in byte array (since each bit represents piece status, piece #0 is at first array index but is bit #7 in the byte)
                    var pieceLoaded = (pieces[i / 8] & (1 << 7 - i % 8)) != 0;
                    if (pieceLoaded)
                    {
                        //piecesDone++;
                        insertPixel(i, 0, 122, 204); //blue

                    }
                    else
                        insertPixel(i, 50, 50, 50); //gray
                }

                for (var i = 0; i < rowCount; i++)
                    unsafe
                    {
                        var rowStart = ((byte*)bitmapData.Scan0.ToPointer() + i * bitmapData.Stride);
                        System.Runtime.InteropServices.Marshal.Copy(rowPixels, 0, new IntPtr(rowStart), rowPixels.Length);
                    }

                // PiecesDone = piecesDone;
                result.UnlockBits(bitmapData);
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
