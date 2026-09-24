using Microsoft.Extensions.DependencyInjection;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Kernel;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the <paramref name="database"/> and the service providers that work on it
	/// </summary>
	/// <param name="database">Database returned by <see cref="MainManager.Initialize"/></param>
	public static IServiceCollection AddKernel(this IServiceCollection services, IDatabase database)
	{
		ArgumentNullException.ThrowIfNull(database);

		services.AddSingleton(database);
		services.AddSingleton<IStockItemServiceProvider, StockItemServiceProvider>();
		services.AddSingleton<ICustomerServiceProvider, CustomerServiceProvider>();
		services.AddSingleton<IInvoiceServiceProvider, InvoiceServiceProvider>();
		services.AddSingleton<IUserServiceProvider, UserServiceProvider>();
		return services;
	}
}
