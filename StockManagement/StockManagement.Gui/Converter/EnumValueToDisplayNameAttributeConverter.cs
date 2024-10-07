using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Gui.Converter;


internal class EnumValueToDisplayNameAttributeConverter : MarkupExtension, IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not Enum enumValue) return string.Empty;

		return EnumExtensions.GetDisplayValue(enumValue);
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