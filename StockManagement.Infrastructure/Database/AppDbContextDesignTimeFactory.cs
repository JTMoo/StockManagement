using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// Lets <c>dotnet ef</c> build <see cref="AppDbContext"/> without a running host
/// </summary>
/// <remarks>Connection string is only needed to generate migrations, not to apply them</remarks>
public sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	public AppDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseNpgsql("Host=localhost;Database=stockmanagement_design")
			.Options;
		return new AppDbContext(options, new ServiceCollection().BuildServiceProvider());
	}
}
