using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace M59AdminTool.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (targetType == typeof(Visibility))
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }

                return boolValue;
            }

            var isNotNull = value != null;
            if (targetType == typeof(Visibility))
            {
                return isNotNull ? Visibility.Visible : Visibility.Collapsed;
            }

            return isNotNull;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
