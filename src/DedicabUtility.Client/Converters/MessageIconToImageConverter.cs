using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using DedicabUtility.Client.Core;

namespace DedicabUtility.Client.Converters
{
    public class MessageIconToImageConverter : IValueConverter
    {
        private const string ICON_SUCCESS = @"pack://application:,,,/DedicabUtility.Client;component/Images/success.png";
        private const string ICON_WARNING = @"pack://application:,,,/DedicabUtility.Client;component/Images/warning.png";
        private const string ICON_ERROR   = @"pack://application:,,,/DedicabUtility.Client;component/Images/error.png";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string uri = ICON_SUCCESS;

            if (value != null)
            {
                var icon = (MessageIcon) value;

                
                switch (icon)
                {
                    case MessageIcon.Success:
                        uri = ICON_SUCCESS;
                        break;
                    case MessageIcon.Warning:
                        uri = ICON_WARNING;
                        break;
                    case MessageIcon.Error:
                        uri = ICON_ERROR;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new BitmapImage(new Uri(uri));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}