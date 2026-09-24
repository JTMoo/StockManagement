using Microsoft.Extensions.DependencyInjection;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Sales.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the sales services. Expects the Kernel service providers to be registered.
	/// </summary>
	public static IServiceCollection AddSalesCore(this IServiceCollection services)
	{
		services.AddSingleton<ISaleService, SaleService>();
		return services;
	}
}
