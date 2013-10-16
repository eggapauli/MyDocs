using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyDocs.WindowsStore.Converter
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime) {
                return new DateTimeOffset((DateTime)value);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset) {
                return ((DateTimeOffset)value).DateTime;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
