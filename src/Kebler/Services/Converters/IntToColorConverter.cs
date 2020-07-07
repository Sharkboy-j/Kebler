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

            //TorHelp.ValidateTorrent(ref torrInf);

            switch (torrInf)
            {
                case -1:
                    return (SolidColorBrush)Application.Current.FindResource("#B0FF3B30");
                case 1:
                   // return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.CheckPendingStrokeBrush");
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFB4B3F1"));
                case 2:
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.CheckingStrokeBrush");
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#B0CB73E1"));

                case 0:
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.StoppedStrokeBrush");
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#E0FF9502"));
                case 4:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#B01CADF8"));

                case 6:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#B064DA38"));
                //return (SolidColorBrush)Application.Current.FindResource("#B064DA38");
                default:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffaacc"));
            }
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

            //TorHelp.ValidateTorrent(ref torrInf);


            switch (torrInf)
            {
                case -1:
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.ErrorFillBrush");
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#40FF3B30"));

                case 1:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#33B4B3F1"));
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.CheckPendingFillBrush");
                case 2:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#40CB73E1"));
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.CheckingFillBrush");
                case 0:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#40FF9502"));
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.StoppedFillBrush");
                case 4:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#401CADF8"));
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.DownloadingFillBrush");
                case 6:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#4064DA38"));
                    //return (SolidColorBrush)Application.Current.FindResource("DataGrid.Tag.UploadingFillBrush");
                default:
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffaacc"));
            }
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

            //TorHelp.ValidateTorrent(ref torrInf);

            switch (torrInf)
            {
                case -1:
                    return Application.Current.FindResource("Icon.Error");
                case 1:
                case 2:
                    return Application.Current.FindResource("Icon.CheckUpdates");
                case 0:
                    return Application.Current.FindResource("Icon.Stopped");
                case 4:
                    return Application.Current.FindResource("Icon.Download");
                case 6:
                    return Application.Current.FindResource("Icon.Upload");
                default:
                    return Application.Current.FindResource("Icon.WTF");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
