using Microsoft.Extensions.DependencyInjection;
using StockManagement.Import.Core.Contracts;

namespace StockManagement.Import.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the import services. Expects the stock item service provider to be registered.
	/// </summary>
	public static IServiceCollection AddImportCore(this IServiceCollection services)
	{
		services.AddScoped<IStockItemImportService, StockItemImportService>();
		services.AddScoped<IImportTargetHandler, StockItemImportTargetHandler>();
		services.AddScoped<IImportTargetHandler, CustomerImportTargetHandler>();
		services.AddScoped<IImportTargetHandler, OpeningStockImportTargetHandler>();
		services.AddScoped<IImportTargetHandler, OpenInvoiceImportTargetHandler>();
		services.AddScoped<IImportBatchService, ImportBatchService>();
		return services;
	}
}
