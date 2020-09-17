using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Kebler.Models.Torrent;

namespace Kebler.Services.Converters
{
    public class PieceToImage : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null && values[1] != null)
            {
                var pices = values[0] as string;

                if (values[1] is int count && count > 1)
                {
                    var piecesGraphic = Utils.CreatePiecesBitmap(count, pices);
                    if (piecesGraphic != null)
                    {
                        var ret = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            piecesGraphic.GetHbitmap(),
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromWidthAndHeight(piecesGraphic.Width, piecesGraphic.Height));
                        return ret;
                    }
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
