using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Transmission.API.RPC.Entity;

namespace Kebler.Services.Converters
{
    public class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TorrentInfo torrInf))
                return null;

            //if (!(value is int num)) return new SolidColorBrush(Color.FromRgb(15, 25, 120));

            //0: 'stopped',
            //1: 'check pending',
            //2: 'checking',
            //3: 'download pending',
            //4: 'downloading',
            //5: 'seed pending',
            //6: 'seeding',
            switch (torrInf.Status)
            {
                case 0:
                    {
                        if (string.IsNullOrEmpty(torrInf.ErrorString))
                            return new SolidColorBrush(Color.FromRgb(225, 138, 208));      //0: 'stopped',
                        else
                            return new SolidColorBrush(Color.FromRgb(187, 22, 0));      //0: 'stopped with error',
                    }

                case 1://check pending
                case 3://download pending
                case 5://seed pending
                    return new SolidColorBrush(Color.FromRgb(255, 255, 0));
                case 2://checking
                    return new SolidColorBrush(Color.FromRgb(248, 167, 59));
                case 4://downloading
                    return new SolidColorBrush(Color.FromRgb(0, 122, 204));
                case 6://seeding
                    return new SolidColorBrush(Color.FromRgb(18, 122, 30));
                default:
                    return new SolidColorBrush(Color.FromRgb(0, 0, 50));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
