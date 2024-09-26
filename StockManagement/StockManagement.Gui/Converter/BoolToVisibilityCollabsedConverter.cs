using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.Converter;


internal class BoolToVisibilityCollabsedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		ArgumentNullException.ThrowIfNull(value);
		if (value is not bool visibility) throw new InvalidCastException(nameof(value));

		if (parameter != null && parameter is string invert && invert.Equals("invert", StringComparison.InvariantCultureIgnoreCase))
			return visibility ? Visibility.Collapsed : Visibility.Visible;

		return visibility ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}