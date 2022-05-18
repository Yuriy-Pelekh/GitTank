﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace GitTank.ValueConverters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class FullPathValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFullPath((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetRelativePath(null, (string)value);
        }
    }
}
