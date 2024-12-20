﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace StockManagement.Gui.Converter;


public class EqualityConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 2) return false;

		if (parameter != null && parameter is string invert && invert.Equals("invert", StringComparison.InvariantCultureIgnoreCase))
			return !values[0].Equals(values[1]);

		return values[0].Equals(values[1]);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
