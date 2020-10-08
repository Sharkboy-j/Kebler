using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for Pieces.xaml
    /// </summary>
    public partial class Pieces : IDisposable
    {
        private byte[] pieces;
        private Bitmap? bmp;
        private int len;
        private Action done;

        ~Pieces()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by an instance of the SprocketControl class and optionally releases the
        ///     managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     'true' to release both managed and unmanaged resources; 'false' to release only unmanaged
        ///     resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            Array.Clear(pieces, 0, pieces.Length);
            bmp?.Dispose();
            bmp = null;
        }

        public void Init(byte[] pieces, int len, Action done)
        {
            this.pieces = pieces;
            this.len = len;
            this.done = done;
            Validate();
        }

        public Pieces() => InitializeComponent();

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            bmp = new Bitmap((int)ActualWidth, (int)ActualHeight);
            Validate();
        }

        private void Validate()
        {
            if(bmp!=null)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    //g.Clear(BackColor);
                    int c_bit = 0, num_bits, bits_got;
                    var bitsperrow = bmp.Width > 0 ? len / (float)bmp.Width : 0;

                    if (bitsperrow > 0)
                    {
                        for (var n = 0; n < bmp.Width; n++)
                        {
                            num_bits = (int)(bitsperrow * (n + 1)) - c_bit;
                            bits_got = 0;
                            for (var i = 0; i < num_bits; i++)
                            {
                                if (BitGet(pieces, len, c_bit + i))
                                    bits_got++;
                            }

                            bool chunk_done;
                            if (num_bits > 0)
                                chunk_done = bits_got / num_bits == 1;
                            else if (BitGet(pieces, len, c_bit))
                                chunk_done = true;
                            else
                                chunk_done = false;

                            var fill = chunk_done ? 
                                  Color.FromArgb(0, 122, 204) :
                                  Color.FromArgb(50, 50, 50);

                            g.DrawLine(new System.Drawing.Pen(fill), n, 0, n, bmp.Height);

                            c_bit += num_bits;
                        }
                    }
                }
                var ret = Imaging.CreateBitmapSourceFromHBitmap(
                    bmp.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
                btmp.Source= ret;
                
                done?.Invoke();
            }
        }

        // private static BitmapImage Convert(Bitmap src)
        // {
        //     MemoryStream ms = new MemoryStream();
        //     src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //     BitmapImage image = new BitmapImage();
        //     image.BeginInit();
        //     ms.Seek(0, SeekOrigin.Begin);
        //     image.StreamSource = ms;
        //     image.EndInit();
        //     return image;
        // }

        private static bool BitGet(byte[] array, int len, int index)
        {
            if (index < 0 || index >= len)
                throw new ArgumentOutOfRangeException();
            return (array[index >> 3] & (1 << ((7 - index) & 7))) != 0;
        }
    }
}
