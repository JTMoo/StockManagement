namespace StockManagement.Kernel.Util;


public static class CommonHelper
{
	public static T? DeepClone<T>(T input)
	{
		if (input == null) return default;

		var type = input.GetType();
		var properties = type.GetProperties();

		if (Activator.CreateInstance(type) is not T clonedObj) return default;

		foreach (var property in properties)
		{
			if (!property.CanWrite) continue;
			if (property.GetValue(input) is not object value) continue;
			if (value.GetType().FullName is not string propertyName) continue;

			if (value != null && value.GetType().IsClass && !propertyName.StartsWith("System."))
			{
				property.SetValue(clonedObj, DeepClone(value));
			}
			else
			{
				property.SetValue(clonedObj, value);
			}
		}

		return clonedObj;
	}
}
