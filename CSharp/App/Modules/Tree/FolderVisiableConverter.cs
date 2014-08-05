using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Modules.Tree
{
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public class FolderVisiableConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }
            bool isFolder = (bool) value;
            if (isFolder)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(
                object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}