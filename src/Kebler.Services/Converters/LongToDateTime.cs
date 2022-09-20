using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class LongToDateTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && long.TryParse(value.ToString(), out var para))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(para).LocalDateTime;
                return dateTimeOffset.ToString("dd.MM.yyyy HH:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = DateTimeOffset.Parse(value.ToString());

            long unixTimeStampInSeconds = date.ToUnixTimeSeconds();
            return unixTimeStampInSeconds;
        }
    }
}