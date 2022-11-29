using MongoDB.Driver;

namespace StockManagement.Kernel;


internal class DatabaseManager
{
	private readonly IMongoDatabase _database;
	private const string ConnectionString = "mongodb://127.0.0.1:27017";
	private const string DatabaseName = "LaCosecha_StockManagement";


	public DatabaseManager(IMongoDatabase database)
	{
		_database = database;
	}
}