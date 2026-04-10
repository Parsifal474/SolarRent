using System;
using System.Globalization;
using System.Windows.Data;

namespace SolarRent.ViewModels
{
    public class BlacklistButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? "Убрать из ЧС" : "В чёрный список";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}