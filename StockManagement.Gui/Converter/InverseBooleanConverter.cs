using System;
using System.Globalization;
using System.Windows.Data;

namespace StockManagement.Gui.Converter;


internal class InverseBooleanConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not bool boolValue) throw new InvalidOperationException("The target must be a boolean");

		return !boolValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
