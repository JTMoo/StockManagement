using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


public class DatabaseManager
{
	private readonly IMongoDatabase _database;
	private const string ConnectionString = "mongodb://127.0.0.1:27017";
	private const string DatabaseName = "LaCosecha_StockManagement";

	internal DatabaseManager()
	{
		// TODO: Dependency Injection ausprogrammieren
		var client = new MongoClient(ConnectionString);
		_database = client.GetDatabase(DatabaseName);
	}


	internal IMongoCollection<Machine> MachineCollection { get; private set; }
	internal IMongoCollection<SparePart> SparePartCollection { get; private set; }
	internal IMongoCollection<Tire> TireCollection { get; private set; }

	internal void Init()
	{
		this.MachineCollection = _database.GetCollection<Machine>(typeof(Machine).ToString());
		this.SparePartCollection = _database.GetCollection<SparePart>(typeof(SparePart).ToString());
		this.TireCollection = _database.GetCollection<Tire>(typeof(Tire).ToString());
	}
}