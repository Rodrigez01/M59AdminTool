using System;
using System.Globalization;
using System.Windows.Data;

namespace M59AdminTool.Converters
{
    public class EditModeToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditMode)
            {
                return isEditMode ? "Edit Quest" : "New Quest";
            }
            return "Quest Editor";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
