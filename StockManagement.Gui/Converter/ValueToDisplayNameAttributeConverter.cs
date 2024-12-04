using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.Converter;


internal class ValueToDisplayNameAttributeConverter : MarkupExtension, IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return TypeExtensions.GetDisplayValue(value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return this;
	}
}