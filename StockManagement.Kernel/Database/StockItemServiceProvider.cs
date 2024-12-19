using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class StockItemServiceProvider(IDatabase database) : IStockItemServiceProvider
{
	private readonly IDatabase _database = database;


	/// <summary>
	/// Tries to add <see cref="StockItem"/> if its unique Property doesn't already exist in the database
	/// </summary>
	/// <param name="invoice"></param>
	/// <returns></returns>
	/// <exception cref="MongoBulkWriteException">Thrown when unique field already exists</exception>
	public async Task AddStockItemAsync(StockItem stockItem)
	{
		var collection = _database.ConnectToMongo<StockItem>();
		await collection.InsertOneAsync(stockItem);

		var addTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount);
		var collection2 = _database.ConnectToMongo<Transaction>();
		await collection2.InsertOneAsync(addTransaction);
		return;
	}

	public Task<DeleteResult> DeleteStockItemAsync(StockItem stockItem)
	{
		var deleteTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1);
		var collection = _database.ConnectToMongo<Transaction>();
		collection.InsertOneAsync(deleteTransaction);
		return _database.Delete<StockItem>(stockItem);
	}

	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
	{
		return _database.GetAll<StockItem>();
	}

	public Task<StockItem> GetStockItemAsync(string code)
	{
		return _database.GetOneAsync<StockItem>(item => item.Code == code);
	}

	public Task<ReplaceOneResult> UpdateStockItemAsync(StockItem stockItem)
	{
		var item = this.GetStockItemAsync(stockItem.Code).Result;
		if (item is StockItem existingItem && existingItem.Amount != stockItem.Amount)
		{
			var changeAmountTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount - item.Amount);
			var collection2 = _database.ConnectToMongo<Transaction>();
			collection2.InsertOneAsync(changeAmountTransaction);
		}

		var collection = _database.ConnectToMongo<StockItem>();
		var filter = Builders<StockItem>.Filter.Eq("Id", stockItem.Code);
		// Upsert means: replace if existent - insert if not existent
		return collection.ReplaceOneAsync(filter, stockItem, new ReplaceOptions { IsUpsert = true });
	}

	public Task AddManyStockItemsAsync(IList<StockItem> stockItems)
	{
		var collection = _database.ConnectToMongo<StockItem>();
		return collection.InsertManyAsync(stockItems);
	}
}
