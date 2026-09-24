using Microsoft.Extensions.DependencyInjection;
using StockManagement.Customers.Core.Contracts;

namespace StockManagement.Customers.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the customer services. Expects the Kernel service providers to be registered.
	/// </summary>
	public static IServiceCollection AddCustomersCore(this IServiceCollection services)
	{
		services.AddSingleton<ICustomerService, CustomerService>();
		return services;
	}
}
