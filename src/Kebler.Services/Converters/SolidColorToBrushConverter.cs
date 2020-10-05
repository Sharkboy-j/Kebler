using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kebler.Services.Converters
{
    public class SolidColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Brush) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}