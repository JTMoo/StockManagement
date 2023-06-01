using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockManagement.Gui.Converter;


internal class BoolToVisibilityCollabsedConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (!(value is bool visibility)) throw new InvalidCastException(nameof(value));

		if (parameter != null && parameter is string invert && invert.ToLowerInvariant() == "invert")
			return visibility ? Visibility.Collapsed : Visibility.Visible;

		return visibility ? Visibility.Visible : Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}