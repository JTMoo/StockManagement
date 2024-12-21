using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.Converter;


internal class NullToVisibilityCollabsedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		var visibility = value != null;
		if (parameter != null && parameter is string invert && invert.Equals("invert", StringComparison.InvariantCultureIgnoreCase))
			return visibility ? Visibility.Collapsed : Visibility.Visible;

		return visibility ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}