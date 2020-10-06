using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class PercentToUserFriendlyConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val) return $"{Math.Round(val * 100, 2)}%";
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}