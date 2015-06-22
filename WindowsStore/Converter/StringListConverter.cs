using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace MyDocs.WindowsStore.Converter
{
    public class StringListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var tags = (IEnumerable<string>)value;
            return string.Join(", ", tags);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var tags = ((string)value).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return tags.Select(tag => tag.Trim()).ToImmutableList();
        }
    }
}
