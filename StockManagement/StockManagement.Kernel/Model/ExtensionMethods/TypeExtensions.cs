using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class TypeExtensions
{
	public static string GetDisplayValue<T>(this T value)
	{
		if (value == null || value.ToString() is not string valueName) return string.Empty;
		var memberInfo = value is MemberInfo ? value as MemberInfo : value.GetType().GetField(valueName);

		if (memberInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) is not DisplayAttribute[] displayAttributes) return valueName;
		if (displayAttributes.Length <= 0 || displayAttributes[0].Name is not string descriptionName) return valueName;
		if (displayAttributes[0].ResourceType is not Type resourceType) return descriptionName;

		return LookupResource(resourceType, descriptionName);
	}

	private static string LookupResource(Type resourceManagerProvider, string resourceKey)
	{
		var resourceKeyProperty = resourceManagerProvider?.GetProperty(resourceKey,
			BindingFlags.Static | BindingFlags.Public, null, typeof(string), [], null);
		if (resourceKeyProperty != null && resourceKeyProperty.GetMethod?.Invoke(null, null) is string resourceName)
		{
			return resourceName;
		}

		return resourceKey;
	}
}
