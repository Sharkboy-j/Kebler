using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class BytesToUserFriendlyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Utils.GetSizeString((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
