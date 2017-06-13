using System;
using System.Globalization;
using System.Windows.Data;

namespace DedicabUtility.Client.Converters
{
    public class DifficultyValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int rating = value as int? ?? -1;

            return rating == -1 ? "-" : rating.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}