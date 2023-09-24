using System;
using System.Globalization;
using System.Windows.Data;

namespace Nimble.Helpers
{
    public class StartupConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "自启";
            return "不自启";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
