using System;
using System.Globalization;
using System.Windows.Data;
using FamFamFam.Flags.Wpf;
using Plarium.Geo.Embedded;
using Plarium.Geo.Services;

namespace Kebler.Services.Converters
{
    public class IpToCountry : IValueConverter
    {
        private static readonly CountryIdToFlagImageSourceConverter Flags = new CountryIdToFlagImageSourceConverter();
        private static readonly GeoService Geo;

        static IpToCountry()
        {
            var builder = new GeoServiceBuilder();
            builder.RegisterResource<EmbeddedResourceReader>();
            Geo = new GeoService(builder);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vl = Geo.ResolveCountry((string) value);
            var img = Flags.Convert(vl, null, null, null);
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}