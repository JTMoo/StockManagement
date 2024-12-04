using System.ComponentModel;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class EnumExtensions
{
	public static string GetEnumDescription(this Enum enumValue)
	{
		var field = enumValue.GetType().GetField(enumValue.ToString());
		if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
		{
			return attribute.Description;
		}

		return string.Empty;
	}
}