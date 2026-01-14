using System;
using System.Globalization;
using System.Windows.Data;
using M59AdminTool.Models;
using M59AdminTool.Services;

namespace M59AdminTool.Converters
{
    public class LocalizedNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WarpLocation warp)
            {
                var localization = LocalizationService.Instance;
                return warp.GetLocalizedName(localization.CurrentLanguage);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
