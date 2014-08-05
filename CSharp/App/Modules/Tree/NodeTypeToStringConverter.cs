using System;
using System.Globalization;
using System.Windows.Data;

namespace Modules.Tree
{
    [ValueConversion(typeof (int), typeof (string))]
    public class NodeTypeToStringConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            return ((NodeType) (int) value).ToString();
        }

        public object ConvertBack(
                object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}