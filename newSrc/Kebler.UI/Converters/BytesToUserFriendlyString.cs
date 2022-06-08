using System;
using System.Globalization;
using System.Windows.Data;
using ByteSizeLib;

namespace Kebler.UI.Converters
{
    public class BytesToUserFriendlyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null && bool.TryParse(parameter.ToString(), out var para))
                return GetSizeString(System.Convert.ToInt64(value), para);

            return GetSizeString((long)value);
        }

        public string Convert(object value)
        {
            return value == null ? string.Empty : GetSizeString((long)value);
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string GetSizeString(long length, bool showEmpty = false)
        {
            var size = ByteSize.FromBytes(length);
            var text = size.ToBinaryString();

            var str = showEmpty ? text : size == new ByteSize(0) ? string.Empty : text;

            return str;

            //long B = 0, KB = 1024, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            //double size = length;
            //var suffix = nameof(B);

            //double SelSize = 0;

            //if (length >= TB)
            //{
            //    SelSize = TB;
            //    suffix = nameof(TB);
            //}
            //else if (length >= GB)
            //{
            //    SelSize = GB;
            //    suffix = nameof(GB);
            //}
            //else if (length >= MB)
            //{
            //    SelSize = MB;
            //    suffix = nameof(MB);
            //}
            //else if (length >= KB)
            //{
            //    SelSize = KB;
            //    suffix = nameof(KB);
            //}
            //else if (length >= B)
            //{
            //    return $"{size} {nameof(B)}";
            //}
            //else
            //{
            //    return showEmpty ? $"0 {nameof(KB)}" : string.Empty;
            //}

            //size = Math.Round(length / SelSize, 2);

            //return $"{size} {suffix}";
        }
    }
}