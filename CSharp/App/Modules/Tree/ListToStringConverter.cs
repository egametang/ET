using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Modules.Tree
{
    [ValueConversion(typeof (List<string>), typeof (string))]
    public class ListToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            var list = (List<string>) value;
            return String.Join(",", list);
        }

        public object ConvertBack(
                object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new List<string>();
            }
            var s = (string) value;
            string[] ss = s.Split(',');
            for (int i = 0; i < ss.Length; ++i)
            {
                ss[i] = ss[i].Trim();
            }
            return ss.ToList();
        }
    }
}