using System;
using System.Globalization;
using System.Windows.Data;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class LevelToIndentationConverter : IValueConverter
    {
        private static readonly double Offset = 10.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var num1 = 0.0;
            if (value is int num2)
                num1 = (num2 - 1) * Offset;
            return num1;
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