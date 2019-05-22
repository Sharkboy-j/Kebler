using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Kebler.Services.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is int num)
            {
                switch(num)
                {
                    case 0://paused
                        return new SolidColorBrush(Color.FromRgb(187, 22, 0));
                    case 3://Queued
                        return new SolidColorBrush(Color.FromRgb(255, 155, 0));
                    case 4://downloading
                        return new SolidColorBrush(Color.FromRgb(0, 122, 204));
                    case 6://seeding
                        return new SolidColorBrush(Color.FromRgb(18, 122, 30));
                    default:
                        return new SolidColorBrush(Color.FromRgb(255, 255, 0));
                }
            }
            return new SolidColorBrush(Color.FromRgb(15, 25, 120));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
