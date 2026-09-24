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

	public async Task<DeleteResult> DeleteStockItemAsync(StockItem stockItem)
	{
		var deleteTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1);
		var collection = _database.ConnectToMongo<Transaction>();
		await collection.InsertOneAsync(deleteTransaction);
		return await _database.Delete<StockItem>(stockItem);
	}

	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
	{
		return _database.GetAll<StockItem>();
	}

	public Task<StockItem> GetStockItemAsync(string code)
	{
		return _database.GetOneAsync<StockItem>(item => item.Code == code);
	}

	public async Task<ReplaceOneResult> UpdateStockItemAsync(StockItem stockItem)
	{
		var item = await _database.GetOneAsync<StockItem>(stored => stored.Id == stockItem.Id);
		if (item is not null && item.Amount != stockItem.Amount)
		{
			await this.SaveTransactionAsync(stockItem, item);
		}

		var collection = _database.ConnectToMongo<StockItem>();
		var filter = Builders<StockItem>.Filter.Eq("Id", stockItem.Id);
		// Upsert means: replace if existent - insert if not existent
		return await collection.ReplaceOneAsync(filter, stockItem, new ReplaceOptions { IsUpsert = true });
	}

	public Task AddManyStockItemsAsync(IList<StockItem> stockItems)
	{
		var collection = _database.ConnectToMongo<StockItem>();
		return collection.InsertManyAsync(stockItems);
	}

	public async Task<StockItem?> TryTakeStockAsync(string code, int amount, CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

		var filter = Builders<StockItem>.Filter.Where(item => item.Code == code && item.Amount >= amount);
		if (await this.IncrementAmountAsync(filter, -amount, cancellationToken) is not StockItem stockItem) return null;

		try
		{
			await this.InsertAmountTransactionAsync(stockItem, -amount, cancellationToken);
		}
		catch
		{
			await this.IncrementAmountAsync(Builders<StockItem>.Filter.Where(item => item.Code == code), amount, CancellationToken.None);
			throw;
		}

		return stockItem;
	}

	public async Task ReturnStockAsync(string code, int amount, CancellationToken cancellationToken = default)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

		if (await this.IncrementAmountAsync(Builders<StockItem>.Filter.Where(item => item.Code == code), amount, cancellationToken) is not StockItem stockItem) return;
		await this.InsertAmountTransactionAsync(stockItem, amount, cancellationToken);
	}

	private Task<StockItem> IncrementAmountAsync(FilterDefinition<StockItem> filter, int amount, CancellationToken cancellationToken)
	{
		var options = new FindOneAndUpdateOptions<StockItem> { ReturnDocument = ReturnDocument.After };
		return _database.ConnectToMongo<StockItem>().FindOneAndUpdateAsync(filter, Builders<StockItem>.Update.Inc(item => item.Amount, amount), options, cancellationToken);
	}

	private Task InsertAmountTransactionAsync(StockItem stockItem, int amount, CancellationToken cancellationToken)
	{
		var transaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, amount);
		return _database.ConnectToMongo<Transaction>().InsertOneAsync(transaction, cancellationToken: cancellationToken);
	}

	private Task SaveTransactionAsync(StockItem stockItem, StockItem item)
	{
		var changeAmountTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount - item.Amount);
		var collection = _database.ConnectToMongo<Transaction>();
		return collection.InsertOneAsync(changeAmountTransaction);
	}
}
