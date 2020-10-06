using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace Kebler.Services.Converters
{
    public class FileTypeToIco : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                var dd = new FileInfo(str);
                if (string.IsNullOrEmpty(dd.Extension)) return Application.Current.FindResource("Icon.Folder");

                var t = $"Icon{dd.Extension.ToLower()}";
                var res = Application.Current.TryFindResource(t);
                return res ?? Application.Current.TryFindResource("Icon.File");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}