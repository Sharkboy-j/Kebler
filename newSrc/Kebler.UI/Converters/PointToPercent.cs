using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.UI.Converters
{
    public class PointToPercent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && double.TryParse(value.ToString(), out var para))
                return $"{Math.Round(para * 100, 2)}%";

            return "~";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

      
    }

 
}