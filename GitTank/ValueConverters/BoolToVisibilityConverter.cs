using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GitTank.ValueConverters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var booleanValue = (bool)value;
            var isInverse = ((string)parameter)?.ToUpper() == "INVERSE";

            if (isInverse)
            {
                booleanValue = !booleanValue;
            }

            var result = booleanValue ? Visibility.Visible : Visibility.Collapsed;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility)value;
            var isInverse = ((string)parameter)?.ToUpper() == "INVERSE";
            var result = visibilityValue == Visibility.Visible;

            if (isInverse)
            {
                result = !result;
            }

            return result;
        }
    }
}
