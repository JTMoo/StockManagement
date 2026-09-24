using Testcontainers.MongoDb;

namespace StockManagement.Api.Tests;


/// <summary>
/// One MongoDB replica set for the test run; transactions need it (ADR-0006)
/// </summary>
[TestClass]
public static class MongoContainer
{
	private static readonly MongoDbContainer _container = new MongoDbBuilder("mongo:7").WithReplicaSet("rs0").Build();


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
