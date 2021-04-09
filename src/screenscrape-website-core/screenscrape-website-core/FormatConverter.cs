using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace screenscrape_website_core
{
    public class FormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            string formatString = parameter as string;
            if (!string.IsNullOrEmpty(formatString))
            {
                return string.Format(
                    new CultureInfo(language), formatString, value);
            }
     
            return value.ToString();
        }

        // No need to implement converting back on a one-way binding 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
