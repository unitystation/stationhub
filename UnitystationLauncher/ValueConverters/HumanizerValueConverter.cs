using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Humanizer;

namespace UnitystationLauncher.ValueConverters
{
    public class HumanizerValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DateTime dt => dt.Humanize(),
                DateTimeOffset dt => dt.Humanize(),
                TimeSpan ts => ts.Humanize(),
                _ => null,
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}