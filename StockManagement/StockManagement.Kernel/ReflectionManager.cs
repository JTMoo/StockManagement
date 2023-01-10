using System.Reflection;

namespace StockManagement.Kernel;


public static class ReflectionManager
{
	public static List<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
	{
		return assembly.GetTypes()
			.Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
			.ToList();
	}
	public static List<Type> GetTypesOfBase(Assembly assembly, Type baseType)
	{
		return assembly.GetTypes()
			.Where(type => type.BaseType != null && type.BaseType == baseType)
			.ToList();
	}
}