using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kebler.UI.Converters
{
    public class IntToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double torrInf)
                return torrInf switch
                {
                    -1 => Application.Current.FindResource("Icon.Error"),
                    1 => Application.Current.FindResource("Icon.Check"),
                    2 => Application.Current.FindResource("Icon.Check"),
                    0 => Application.Current.FindResource("Icon.Stopped"),
                    4 => Application.Current.FindResource("Icon.Download"),
                    6 => Application.Current.FindResource("Icon.Upload"),
                    _ => Application.Current.FindResource("Icon.Wtf")
                };
            return new object();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}