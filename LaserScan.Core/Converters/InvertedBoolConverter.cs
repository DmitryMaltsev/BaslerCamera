using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value is Visibility v && v == Visibility.Collapsed;
        }
    }
}
