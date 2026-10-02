using Microsoft.Extensions.DependencyInjection;
using StockManagement.Auth.Core.Contracts;

namespace StockManagement.Auth.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the auth services. Expects the Kernel service providers to be registered.
	/// </summary>
	public static IServiceCollection AddAuthCore(this IServiceCollection services)
	{
		services.AddScoped<IAuthService, AuthService>();
		return services;
	}
}
