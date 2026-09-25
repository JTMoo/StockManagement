using System.Linq;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Auth.Core;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Customers.Core;
using StockManagement.Customers.Core.Contracts;
using StockManagement.Import.Core;
using StockManagement.Import.Core.Contracts;
using StockManagement.Infrastructure;
using StockManagement.Infrastructure.Database;
using StockManagement.Sales.Core;
using StockManagement.Sales.Core.Contracts;
using StockManagement.Settings.Core;
using StockManagement.Settings.Core.Contracts;

var builder = WebApplication.CreateBuilder(args);

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing.");

builder.Services
	.AddAuthenticationJwtBearer(options => options.SigningKey = jwtSigningKey)
	.AddAuthorization()
	.AddFastEndpoints()
	.AddInfrastructure(builder.Configuration)
	.AddInfrastructureServiceProviders()
	.AddSalesCore()
	.AddCustomersCore()
	.AddSettingsCore()
	.AddImportCore()
	.AddAuthCore()
	.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// AddSalesCore/AddCustomersCore/AddSettingsCore/AddAuthCore register these Singleton for the GUI's Mongo Kernel providers;
// the API's providers above are Scoped (EF's AppDbContext isn't thread-safe), so override to match
MakeScoped<ISaleService>(builder.Services);
MakeScoped<ICustomerService>(builder.Services);
MakeScoped<ISettingsService>(builder.Services);
MakeScoped<IStockItemImportService>(builder.Services);
MakeScoped<IAuthService>(builder.Services);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
	await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

// React build (StockManagement.Web) lands in wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(config =>
{
	config.Endpoints.RoutePrefix = "api";
	config.Errors.UseProblemDetails();
	config.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});

await app.RunAsync();


/// <summary>
/// Entry point, public for <c>WebApplicationFactory</c>
/// </summary>
public partial class Program
{
	/// <summary>
	/// Swaps an already-registered service's lifetime to Scoped, by reflection so the (internal) implementation type doesn't need to be named
	/// </summary>
	private static void MakeScoped<TService>(IServiceCollection services)
	{
		var descriptor = services.Single(d => d.ServiceType == typeof(TService));
		services.Remove(descriptor);
		services.Add(new ServiceDescriptor(typeof(TService), descriptor.ImplementationType!, ServiceLifetime.Scoped));
	}
}
