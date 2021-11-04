using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Kebler.Models;

namespace Kebler.Services.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is TorrentFile trnt)
            {
                return trnt.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}