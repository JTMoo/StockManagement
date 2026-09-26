using Microsoft.Extensions.DependencyInjection;
using StockManagement.Import.Core.Contracts;

namespace StockManagement.Import.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the import services. Expects the stock item service provider to be registered.
	/// </summary>
	/// <remarks>
	/// Singleton here, matching the GUI's Singleton <c>IStockItemServiceProvider</c>; the API's providers are Scoped
	/// (EF's <c>AppDbContext</c> isn't thread-safe), so it swaps <see cref="IStockItemImportService"/> to Scoped too, same as <c>ISaleService</c>/<c>ICustomerService</c>/<c>ISettingsService</c>.
	/// </remarks>
	public static IServiceCollection AddImportCore(this IServiceCollection services)
	{
		services.AddSingleton<IStockItemImportService, StockItemImportService>();
		services.AddSingleton<IExcelStockItemParser, ExcelStockItemParser>();
		services.AddSingleton<IImportTargetHandler, StockItemImportTargetHandler>();
		services.AddSingleton<IImportTargetHandler, CustomerImportTargetHandler>();
		services.AddSingleton<IImportBatchService, ImportBatchService>();
		return services;
	}
}
