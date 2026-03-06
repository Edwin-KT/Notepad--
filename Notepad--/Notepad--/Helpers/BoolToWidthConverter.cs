using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Notepad__.Helpers
{
    // Converteste true/false intr-o latime de coloana
    // true  => 250px (exploratorul e vizibil)
    // false => 0px   (exploratorul e ascuns)
    public class BoolToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? new GridLength(250) : new GridLength(0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}