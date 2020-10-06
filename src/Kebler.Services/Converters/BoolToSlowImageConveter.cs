using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class BoolToSlowImageConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var res = (bool) value
                ? Application.Current.FindResource("Icon.SnailColor")
                : Application.Current.FindResource("Icon.SnailFill");
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}