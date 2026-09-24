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

	public async Task<int> DeleteStockItemAsync(StockItem stockItem)
	{
		var deleteTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1);
		var collection = _database.ConnectToMongo<Transaction>();
		await collection.InsertOneAsync(deleteTransaction);
		var result = await _database.Delete<StockItem>(stockItem);
		return (int)result.DeletedCount;
	}

	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
	{
		return _database.GetAll<StockItem>();
	}

	public Task<StockItem> GetStockItemAsync(string code)
	{
		return _database.GetOneAsync<StockItem>(item => item.Code == code);
	}

	public async Task<int> UpdateStockItemAsync(StockItem stockItem)
	{
		var item = await _database.GetOneAsync<StockItem>(stored => stored.Id == stockItem.Id);
		if (item is not null && item.Amount != stockItem.Amount)
		{
			await this.SaveTransactionAsync(stockItem, item);
		}

		var collection = _database.ConnectToMongo<StockItem>();
		var filter = Builders<StockItem>.Filter.Eq("Id", stockItem.Id);
		// Upsert means: replace if existent - insert if not existent
		var result = await collection.ReplaceOneAsync(filter, stockItem, new ReplaceOptions { IsUpsert = true });
		return (int)result.ModifiedCount;
	}

	public Task AddManyStockItemsAsync(IList<StockItem> stockItems)
	{
		var collection = _database.ConnectToMongo<StockItem>();
		return collection.InsertManyAsync(stockItems);
	}

	private Task SaveTransactionAsync(StockItem stockItem, StockItem item)
	{
		var changeAmountTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount - item.Amount);
		var collection = _database.ConnectToMongo<Transaction>();
		return collection.InsertOneAsync(changeAmountTransaction);
	}
}
