using Microsoft.Extensions.DependencyInjection;
using StockManagement.Sifen.Core.Contracts;

namespace StockManagement.Sifen.Core;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the Sifen services (CDC generation, DTE XML building, XAdES signing, XSD validation). No transmission gateway — see #133.
	/// </summary>
	public static IServiceCollection AddSifenCore(this IServiceCollection services)
	{
		services.AddScoped<ICdcGenerator, CdcGenerator>();
		services.AddScoped<IDteXmlBuilder, DteXmlBuilder>();
		services.AddScoped<IXadesSigner, XadesSigner>();
		services.AddScoped<IDteXsdValidator, DteXsdValidator>();
		return services;
	}
}
