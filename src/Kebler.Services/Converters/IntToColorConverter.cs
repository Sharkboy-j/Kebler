using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Kebler.Services.Converters
{
    //0: 'stopped',
    //1: 'check pending',
    //2: 'checking',
    //3: 'download pending',
    //4: 'downloading',
    //5: 'seed pending',
    //6: 'seeding',

    public class IntToColorStrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double torrInf))
                return null;

            return torrInf switch
            {
                -1 => (SolidColorBrush) new BrushConverter().ConvertFrom("#CA2327"),
                1 => (SolidColorBrush) new BrushConverter().ConvertFrom("#FFB4B3F1"),
                2 => (SolidColorBrush) new BrushConverter().ConvertFrom("#B0CB73E1"),
                0 => (SolidColorBrush) new BrushConverter().ConvertFrom("#E0FF9502"),
                4 => (SolidColorBrush) new BrushConverter().ConvertFrom("#B01CADF8"),
                6 => (SolidColorBrush) new BrushConverter().ConvertFrom("#B064DA38"),
                _ => (SolidColorBrush) new BrushConverter().ConvertFrom("#ffaacc")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToColorFillConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double torrInf))
                return null;
            return torrInf switch
            {
                -1 => (SolidColorBrush) new BrushConverter().ConvertFrom("#40FF3B30"),
                1 => (SolidColorBrush) new BrushConverter().ConvertFrom("#33B4B3F1"),
                2 => (SolidColorBrush) new BrushConverter().ConvertFrom("#40CB73E1"),
                0 => (SolidColorBrush) new BrushConverter().ConvertFrom("#40FF9502"),
                4 => (SolidColorBrush) new BrushConverter().ConvertFrom("#401CADF8"),
                6 => (SolidColorBrush) new BrushConverter().ConvertFrom("#4064DA38"),
                _ => (SolidColorBrush) new BrushConverter().ConvertFrom("#ffaacc")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double torrInf))
                return null;

            return torrInf switch
            {
                -1 => Application.Current.FindResource("Icon.Error"),
                1 => Application.Current.FindResource("Icon.Check"),
                2 => Application.Current.FindResource("Icon.Check"),
                0 => Application.Current.FindResource("Icon.Stopped"),
                4 => Application.Current.FindResource("Icon.Download"),
                6 => Application.Current.FindResource("Icon.Upload"),
                _ => Application.Current.FindResource("Icon.WTF")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}