using Microsoft.Extensions.DependencyInjection;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Settings.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the settings services. Expects the Kernel service providers to be registered.
	/// </summary>
	public static IServiceCollection AddSettingsCore(this IServiceCollection services)
	{
		services.AddSingleton<ISettingsService, SettingsService>();
		return services;
	}
}
