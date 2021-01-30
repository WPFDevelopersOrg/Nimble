using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SoftwareHelper.Helpers
{
    public class DarkAndLightConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string rel = "";
            if (value == null)
                rel = "Dark";
            else
            {
                if ((bool)value)
                {
                    rel = "Light"; 
                }
                else
                {
                    rel = "Dark";
                }
            }
            return rel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
