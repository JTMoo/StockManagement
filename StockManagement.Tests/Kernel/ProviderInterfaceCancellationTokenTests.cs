using System.Reflection;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Tests.Kernel;


/// <summary>Guard for issue #152: a new provider method without CancellationToken regresses silently otherwise</summary>
[TestClass]
public sealed class ProviderInterfaceCancellationTokenTests
{
	[TestMethod]
	public void AsyncProviderMethods_AllTakeCancellationToken()
	{
		var violations = typeof(ICustomerServiceProvider).Assembly.GetTypes()
			.Where(type => type.IsInterface && type.Namespace == typeof(ICustomerServiceProvider).Namespace && type.Name.EndsWith("ServiceProvider"))
			.SelectMany(type => type.GetMethods())
			.Where(method => typeof(Task).IsAssignableFrom(method.ReturnType))
			.Where(method => !method.GetParameters().Any(parameter => parameter.ParameterType == typeof(CancellationToken)))
			.Select(method => $"{method.DeclaringType!.Name}.{method.Name}")
			.ToList();

		Assert.IsTrue(violations.Count == 0, $"Missing CancellationToken parameter: {string.Join(", ", violations)}");
	}
}
