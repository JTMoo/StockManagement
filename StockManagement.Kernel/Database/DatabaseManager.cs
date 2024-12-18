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
		var collection = ConnectToMongo<T>();
		var list = await collection.FindAsync(_ => true);
		return list.ToEnumerable();
	}

	public async Task<T> GetOneAsync<T>(Expression<Func<T, bool>> filter)
	{
		var collection = ConnectToMongo<T>();
		var element = await collection.FindAsync(filter);
		return element.FirstOrDefault();
	}

	public Task<DeleteResult> Delete<T>(BaseDocument item)
	{
		var col = ConnectToMongo<T>();
		var filter = Builders<T>.Filter.Eq("Id", item.Id);

		return col.DeleteOneAsync(filter);
	}

	public IMongoCollection<T> ConnectToMongo<T>(in string collectionName)
	{
		var client = new MongoClient(ConnectionString);
		var db = client.GetDatabase(DatabaseName);
		return db.GetCollection<T>(collectionName);
	}

	public IMongoCollection<T> ConnectToMongo<T>()
	{
		var client = new MongoClient(ConnectionString);
		var db = client.GetDatabase(DatabaseName);
		return db.GetCollection<T>(typeof(T).ToString());
	}
}