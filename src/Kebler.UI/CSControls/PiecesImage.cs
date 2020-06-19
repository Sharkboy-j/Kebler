using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Kebler.UI.CSControls
{
    public class PiecesImage : Image
    {
        public  readonly DependencyProperty PieceCountProperty = DependencyProperty.Register(nameof(PieceCount), typeof(uint), typeof(PiecesImage),
            new FrameworkPropertyMetadata(0U, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public uint PieceCount
        {
            get => (uint)GetValue(PieceCountProperty);
            set => SetValue(PieceCountProperty, value);
        }

        public  readonly DependencyProperty PiecesProperty = DependencyProperty.Register(nameof(Pieces), typeof(string), typeof(PiecesImage),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));




        public string Pieces
        {
            get => (string)GetValue(PiecesProperty);
            set => SetValue(PiecesProperty, value);
        }

        void init()
        {
            var piecesGraphic = createPiecesBitmap(PieceCount, Pieces);
            Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                piecesGraphic.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(piecesGraphic.Width, piecesGraphic.Height));
        }

        static Bitmap createPiecesBitmap(uint pieceCount, string piecesString)
        {
            //int piecesDone = 0;

            if (pieceCount < 1)
                return null;

            var pieces = Convert.FromBase64String(piecesString);


            const int rowCount = 100;
            var result = new Bitmap((int)pieceCount, rowCount, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var bitmapData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, result.PixelFormat);
            var rowPixels = new byte[bitmapData.Stride];


            //yea yea yea, go further and call me idiot later @Shark
            void InsertPixel(int index, byte r, byte g, byte b)
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
                    InsertPixel(i, 0, 122, 204); //blue
                }
                else
                    InsertPixel(i, 50, 50, 50); //gray
            }

            for (var i = 0; i < rowCount; i++)
                unsafe
                {
                    var rowStart = (byte*)bitmapData.Scan0.ToPointer() + i * bitmapData.Stride;
                    System.Runtime.InteropServices.Marshal.Copy(rowPixels, 0, new IntPtr(rowStart), rowPixels.Length);
                }

            result.UnlockBits(bitmapData);
            return result;
        }
    }
}
