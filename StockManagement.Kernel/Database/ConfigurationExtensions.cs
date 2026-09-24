using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public static class ConfigurationExtensions
{
	/// <summary>
	/// Opens the database named by <c>Mongo:DatabaseName</c> on <c>ConnectionStrings:Mongo</c>
	/// </summary>
	/// <remarks>Call once per process: the driver expects one shared <see cref="MongoClient"/>.</remarks>
	/// <exception cref="InvalidOperationException">A key is missing</exception>
	public static IMongoDatabase GetMongoDatabase(this IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("Mongo") ?? throw new InvalidOperationException("ConnectionStrings:Mongo is missing.");
		var databaseName = configuration["Mongo:DatabaseName"] ?? throw new InvalidOperationException("Mongo:DatabaseName is missing.");
		return new MongoClient(connectionString).GetDatabase(databaseName);
	}
}
