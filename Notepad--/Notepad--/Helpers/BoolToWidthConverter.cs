using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Notepad__.Helpers
{
    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? new GridLength(250) : new GridLength(0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}