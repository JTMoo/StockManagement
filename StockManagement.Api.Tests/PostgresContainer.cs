using Testcontainers.PostgreSql;

namespace StockManagement.Api.Tests;


/// <summary>
/// One PostgreSQL server for the test run; each test gets its own database on it
/// </summary>
[TestClass]
public static class PostgresContainer
{
	private static readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16").Build();


	public static string ConnectionString => _container.GetConnectionString();


	[AssemblyInitialize]
	public static Task StartAsync(TestContext _)
	{
		return _container.StartAsync();
	}

	[AssemblyCleanup]
	public static ValueTask StopAsync()
	{
		return _container.DisposeAsync();
	}
}
