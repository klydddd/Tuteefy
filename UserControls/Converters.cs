using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TuteefyWPF.UserControls
{
    // Converts multiple values to background brush for selected/today dates
    public class DateSelectedMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 || !(values[0] is DateTime) || !(values[1] is DateTime))
                return Brushes.Transparent;

            DateTime date = (DateTime)values[0];
            DateTime selected = (DateTime)values[1];
            bool isCurrent = values[2] is bool && (bool)values[2];
            DateTime today = (DateTime)values[3];

            if (date == selected)
                return new SolidColorBrush(Color.FromRgb(150, 0, 255)); // Selected date color
            if (date == today)
                return new SolidColorBrush(Color.FromRgb(230, 230, 250)); // Today
            if (!isCurrent)
                return new SolidColorBrush(Color.FromRgb(240, 240, 240)); // Other month days

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    // Converts boolean to Visibility
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool && (bool)value;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility visibility) && visibility == Visibility.Visible;
        }
    }
}
