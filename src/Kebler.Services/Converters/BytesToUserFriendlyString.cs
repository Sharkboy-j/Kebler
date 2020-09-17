using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class BytesToUserFriendlyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           

            if (parameter != null && bool.TryParse(parameter.ToString(), out var para))
            {
                return Utils.GetSizeString((long)value,para);
            }

            return Utils.GetSizeString((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
