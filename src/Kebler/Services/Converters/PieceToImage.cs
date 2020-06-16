using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Kebler.Models.Torrent;

namespace Kebler.Services.Converters
{
    public class PieceToImage : IValueConverter
    {
        private static Bitmap CreatePiecesBitmap(int pieceCount, string piecesString)
        {
            //int piecesDone = 0;

            if (pieceCount < 1)
                return null;

            byte[] pieces = System.Convert.FromBase64String(piecesString);


            int rowCount = 100;
            Bitmap result = new Bitmap(pieceCount, rowCount, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var bitmapData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);
            byte[] rowPixels = new byte[bitmapData.Stride];


            //yea yea yea, go further and call me idiot later @Shark
            void insertPixel(int index, byte r, byte g, byte b)
            {
                rowPixels[index * 3] = b;
                rowPixels[index * 3 + 1] = g;
                rowPixels[index * 3 + 2] = r;
            }

            for (int i = 0; i < pieceCount; i++)
            {
                // read bit at specific place in byte array (since each bit represents piece status, piece #0 is at first array index but is bit #7 in the byte)
                bool pieceLoaded = (pieces[i / 8] & (1 << 7 - i % 8)) != 0;
                if (pieceLoaded)
                {
                    //piecesDone++;
                    insertPixel(i, 0, 122, 204); //blue

                }
                else
                    insertPixel(i, 50, 50, 50); //gray
            }

            for (int i = 0; i < rowCount; i++)
                unsafe
                {
                    byte* rowStart = ((byte*)bitmapData.Scan0.ToPointer() + i * bitmapData.Stride);
                    System.Runtime.InteropServices.Marshal.Copy(rowPixels, 0, new IntPtr(rowStart), rowPixels.Length);
                }

            // PiecesDone = piecesDone;
            result.UnlockBits(bitmapData);
            return result;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TorrentInfo tor)
            {
                var piecesGraphic = CreatePiecesBitmap(tor.PieceCount, tor.Pieces);
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                piecesGraphic.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(piecesGraphic.Width, piecesGraphic.Height));
            }
            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
