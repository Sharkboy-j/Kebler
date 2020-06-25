using System;
using System.Globalization;
using System.Windows.Data;
using FamFamFam.Flags.Wpf;

namespace Kebler.Services.Converters
{
    public class IpToCountry : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vl = App.Instance.Geo.ResolveCountry((string)value);
            var img = App.Instance.Flags.Convert(vl,null,null,null);
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}