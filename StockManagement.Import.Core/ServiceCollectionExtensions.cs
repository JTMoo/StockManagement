using Microsoft.Extensions.DependencyInjection;
using StockManagement.Import.Core.Contracts;

namespace StockManagement.Import.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the import services. Expects the Kernel service providers to be registered.
	/// </summary>
	public static IServiceCollection AddImportCore(this IServiceCollection services)
	{
		services.AddSingleton<IStockItemImportService, StockItemImportService>();
		return services;
	}
}
