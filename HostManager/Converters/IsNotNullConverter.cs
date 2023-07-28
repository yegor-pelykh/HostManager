﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Net;
using System.Windows;

namespace HostManager.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
