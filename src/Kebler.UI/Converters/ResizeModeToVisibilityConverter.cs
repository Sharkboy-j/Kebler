using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kebler.UI.Converters
{
    public class ResizeModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (object)(Visibility)((ResizeMode)value == ResizeMode.CanResizeWithGrip ? 0 : 2);
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
