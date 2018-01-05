using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DedicabUtility.Client.Models;

namespace DedicabUtility.Client.Converters
{
    public class PickStateToVisibilityConverter : IValueConverter
    {
        public PickState VisibleState { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PickState state)
            {
                return state == VisibleState ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}