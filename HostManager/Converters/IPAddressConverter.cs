using System;
using System.Globalization;
using System.Windows.Data;
using System.Net;

namespace HostManager.Converters
{
    [ValueConversion(typeof(IPAddress), typeof(string))]
    public class IpAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IPAddress address)
                return null;

            return address.ToString();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string str)
                return null;

            return IPAddress.TryParse(str.Trim(), out var address)
                ? address
                : null;
        }

    }

}
