using System;
using System.Globalization;
using System.Windows.Data;
using Kebler.Resources;

namespace Kebler.Services.Converters
{
    public class TI_StateConverter : IValueConverter
    {
        //0: 'stopped',
        //1: 'check pending',
        //2: 'checking',
        //3: 'download pending',
        //4: 'downloading',
        //5: 'seed pending',
        //6: 'seeding',
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out var val))
            {
                return val switch
                {
                    0 => Strings.TI_Status_Stopped,
                    1 => Strings.TI_Status_CheckPending,
                    2 => Strings.TI_Status_Checking,
                    3 => Strings.TI_Status_download_pending,
                    4 => Strings.TI_Status_Downloading,
                    5 => Strings.TI_Status_SeedPending,
                    6 => Strings.TI_Status_Seeding,
                    _ => string.Empty,
                };   
            }
            return string.Empty;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}