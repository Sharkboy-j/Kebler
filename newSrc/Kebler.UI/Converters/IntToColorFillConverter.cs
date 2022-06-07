using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kebler.UI.Converters
{
    public class IntToColorFillConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double torrInf)
                return torrInf switch
                {
                    -1 => (SolidColorBrush)new BrushConverter().ConvertFrom("#40FF3B30"),
                    1 => (SolidColorBrush)new BrushConverter().ConvertFrom("#33B4B3F1"),
                    2 => (SolidColorBrush)new BrushConverter().ConvertFrom("#40CB73E1"),
                    0 => (SolidColorBrush)new BrushConverter().ConvertFrom("#40FF9502"),
                    4 => (SolidColorBrush)new BrushConverter().ConvertFrom("#401CADF8"),
                    6 => (SolidColorBrush)new BrushConverter().ConvertFrom("#4064DA38"),
                    _ => (SolidColorBrush)new BrushConverter().ConvertFrom("#ffaacc")
                };
            return new object();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}