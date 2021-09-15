using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class WindowStateToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ws = (WindowState)value;
            Visibility v = Visibility.Visible;
            switch (ws)
            {
                case WindowState.Normal:
                    v = Visibility.Visible;
                    break;
                case WindowState.Minimized:
                    v = Visibility.Visible;
                    break;
                case WindowState.Maximized:
                    v = Visibility.Collapsed;
                    break;
            }
            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
