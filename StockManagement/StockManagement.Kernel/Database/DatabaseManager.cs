using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public class DatabaseManager
{
    private const string ConnectionString = "mongodb://127.0.0.1:27017";
    private const string DatabaseName = "LaCosecha_StockManagement";


	internal static async Task<List<T>> GetAll<T>()
	{
		var collection = ConnectToMongo<T>(typeof(T).ToString());
		var list = await collection.FindAsync(_ => true);
		return list.ToList();
	}

	internal static T GetFirst<T>()
	{
		var settingsCollection = ConnectToMongo<T>(typeof(T).ToString());
		var settings = settingsCollection.Find(_ => true);
		return settings.FirstOrDefault();
	}

	internal static Task Add<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		return col.InsertOneAsync(item);
	}

	internal static Task Delete<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		var filter = Builders<BaseDocument>.Filter.Eq("Id", item.Id);

		return col.DeleteOneAsync(filter);
	}

	internal static Task Update<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		var filter = Builders<BaseDocument>.Filter.Eq("Id", item.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, item, new ReplaceOptions { IsUpsert = true });
	}

	internal static IMongoCollection<T> ConnectToMongo<T>(in string collectionName)
	{
		var client = new MongoClient(ConnectionString);
		var db = client.GetDatabase(DatabaseName);
		return db.GetCollection<T>(collectionName);
	}
}