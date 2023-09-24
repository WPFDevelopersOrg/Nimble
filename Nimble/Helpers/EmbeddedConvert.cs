using System;
using System.Globalization;
using System.Windows.Data;

namespace Nimble.Helpers
{
    public class EmbeddedConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "关闭嵌入";
            return "开启嵌入";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}