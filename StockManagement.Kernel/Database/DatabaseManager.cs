using System.Linq.Expressions;
using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Kernel.Database;


public class DatabaseManager : IDatabase
{
    private const string ConnectionString = "mongodb://127.0.0.1:27017";
    private const string DatabaseName = "LaCosecha_StockManagement";


	public async Task<IEnumerable<T>> GetAll<T>()
	{
		var collection = ConnectToMongo<T>(typeof(T).ToString());
		var list = await collection.FindAsync(_ => true);
		return list.ToEnumerable();
	}

	public async Task<T> GetOneAsync<T>(Expression<Func<T, bool>> filter)
	{
		var collection = ConnectToMongo<T>(typeof(T).ToString());
		var element = await collection.FindAsync(filter);
		return element.FirstOrDefault();
	}

	public Task Add<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		return col.InsertOneAsync(item);
	}

	public Task<DeleteResult> Delete<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		var filter = Builders<BaseDocument>.Filter.Eq("Id", item.Id);

		return col.DeleteOneAsync(filter);
	}

	public Task<ReplaceOneResult> Update<T>(BaseDocument item)
	{
		var col = ConnectToMongo<BaseDocument>(typeof(T).ToString());
		var filter = Builders<BaseDocument>.Filter.Eq("Id", item.Id);
		// Upsert means: replace if existent - insert if not existent
		return col.ReplaceOneAsync(filter, item, new ReplaceOptions { IsUpsert = true });
	}

	public IMongoCollection<T> ConnectToMongo<T>(in string collectionName)
	{
		var client = new MongoClient(ConnectionString);
		var db = client.GetDatabase(DatabaseName);
		return db.GetCollection<T>(collectionName);
	}
}