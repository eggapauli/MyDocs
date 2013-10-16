using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MyDocs.WindowsStore.Common.Converter
{
	public class DateToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			DateTime date = (DateTime)value;
			return date.ToString("d");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			string strValue = value as string;

			DateTime resultDateTime;
			if (DateTime.TryParseExact(strValue, "d", null, DateTimeStyles.None, out resultDateTime)) {
				return resultDateTime;
			}
			return DependencyProperty.UnsetValue;
		}
	}

}
