using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Linq;

namespace StockManagement.Gui.Converter;


internal class MultiBoolToVisibilityCollabsedConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		ArgumentNullException.ThrowIfNull(values);
		if (values.Any(value => value is not bool)) return values;

		var visibility = values.All(value => (bool)value);
		if (parameter != null && parameter is string invert && invert.Equals("invert", StringComparison.InvariantCultureIgnoreCase))
			return visibility ? Visibility.Collapsed : Visibility.Visible;

		return visibility ? Visibility.Visible : Visibility.Collapsed;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
