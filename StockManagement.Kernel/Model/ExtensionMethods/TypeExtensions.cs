using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class TypeExtensions
{
	/// <summary>
	/// Tries to convert a <see cref="string"/> to a given <see cref="Type"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="type"></param>
	/// <param name="original"></param>
	/// <param name="converted"></param>
	/// <returns><see cref="true"/>, if the conversion was successfull</returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="NotSupportedException"></exception>
	public static bool TryConvertFromString(this Type type, string original, out object converted)
	{
		converted = new();

		if (type is null) return false;
		if (TypeDescriptor.GetConverter(type).ConvertFromString(original) is not object newValue) return false;

		converted = newValue;
		return true;
	}

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
