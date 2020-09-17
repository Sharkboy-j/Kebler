using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class BytesToUserFriendlySpeed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (parameter != null && bool.TryParse(parameter.ToString(), out var para))
            {
                return GetSizeString(System.Convert.ToInt64(value),para);
            }

            return GetSizeString(System.Convert.ToInt64(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        public static string GetSizeString(long length, bool showEmpty=false)
        {
            long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double size = length;
            var suffix = nameof(B);

            double SelSize;
            if (length >= TB)
            {
                SelSize = TB;
                suffix = nameof(TB);
            }
            else if (length >= GB)
            {
                SelSize = GB;
                suffix = nameof(GB);
            }
            else if (length >= MB)
            {
                SelSize = MB;
                suffix = nameof(MB);
            }
            else if (length >= KB)
            {
                SelSize = KB;
                suffix = nameof(KB);
            }
            else
            {
                return showEmpty ? $"0 KB/s" : string.Empty;
            }

            size = Math.Round(length / SelSize, 2);

            return $"{size} {suffix}/s";
        }

    }
}