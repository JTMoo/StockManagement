using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Infrastructure.Database;

namespace StockManagement.Infrastructure;


public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers <see cref="AppDbContext"/> on <c>ConnectionStrings:Postgres</c>
	/// </summary>
	/// <exception cref="InvalidOperationException">The connection string is missing</exception>
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
		return services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
	}
}
