using System;
using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace WijkMeld.App.Converters
{
    public class NumberToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0;
            }
            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }
            return false; // Standaard false als de waarde niet numeriek of een collectie is
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Niet van toepassing voor deze converter in de meeste UI-scenario's
            throw new NotImplementedException();
        }
    }

}
