using System.Linq.Expressions;
using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class DatabaseManager(IMongoDatabase database) : IDatabase
{
	private readonly IMongoDatabase _database = database;


	/// <summary>
	/// Creates the unique indexes on the business keys
	/// </summary>
	/// <remarks>Idempotent; existing indexes are kept.</remarks>
	public async Task CreateUniqueIndexesAsync(CancellationToken cancellationToken = default)
	{
		await this.ConnectToMongo<StockItem>().Indexes.CreateManyAsync(UniquePropertyHelper.GetStockItemUniqueProperties(), cancellationToken);
		await this.ConnectToMongo<Customer>().Indexes.CreateManyAsync(UniquePropertyHelper.GetCustomerUniqueProperties(), cancellationToken);
		await this.ConnectToMongo<Invoice>().Indexes.CreateManyAsync(UniquePropertyHelper.GetInvoiceUniqueProperties(), cancellationToken);
	}

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
		return _database.GetCollection<T>(collectionName);
	}

	public IMongoCollection<T> ConnectToMongo<T>()
	{
		return _database.GetCollection<T>(typeof(T).ToString());
	}
}