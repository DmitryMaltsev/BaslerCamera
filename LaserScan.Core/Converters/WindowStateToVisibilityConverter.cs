using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class WindowStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ws = (WindowState)value;
            Visibility v = Visibility.Collapsed;
            switch (ws)
            {
                case WindowState.Normal:
                    v = Visibility.Collapsed;
                    break;
                case WindowState.Minimized:
                    v = Visibility.Collapsed;
                    break;
                case WindowState.Maximized:
                    v = Visibility.Visible;
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
