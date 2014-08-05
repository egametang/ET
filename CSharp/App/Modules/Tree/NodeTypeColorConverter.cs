using System;
using System.Globalization;
using System.Windows.Data;

namespace Modules.Tree
{
    [ValueConversion(typeof (int), typeof (int))]
    public class NodeTypeColorConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            int nodeType = (int) value;
            if (nodeType == (int) NodeType.Selector)
            {
                return "selector";
            }
            if (nodeType == (int) NodeType.Sequence)
            {
                return "sequence";
            }
            if (nodeType < 20000 && nodeType >= 10000)
            {
                return "condition";
            }
            if (nodeType < 30000 && nodeType >= 20000)
            {
                return "action";
            }
            return "other";
        }

        public object ConvertBack(
                object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}