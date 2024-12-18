using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class StockItemServiceProvider(IDatabase database) : IStockItemServiceProvider
{
	private readonly IDatabase _database = database;


	public async Task AddStockItemAsync(StockItem stockItem)
	{
		await _database.Add<StockItem>(stockItem);

		var addTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount);
		await _database.Add<Transaction>(addTransaction);
		return;
	}

	public Task<DeleteResult> DeleteStockItemAsync(StockItem stockItem)
	{
		var deleteTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1);
		_database.Add<Transaction>(deleteTransaction);
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
			_database.Add<Transaction>(changeAmountTransaction);
		}

		return _database.Update<StockItem>(stockItem);
	}

	public Task AddManyStockItemsAsync(IList<StockItem> stockItems)
	{
		var collection = _database.ConnectToMongo<StockItem>();
		return collection.InsertManyAsync(stockItems);
	}

	public async Task<int> RemoveDuplicates(List<StockItem> stockItems)
	{
		var existingItems = await this.GetAllStockItemsAsync().ContinueWith(task => task.Result.ToList());
		var count = 0;
		stockItems = stockItems.Where(item =>
		{
			if (existingItems.Any(existingItem => item.Code == existingItem.Code))
			{
				count++;
				return false;
			}

			return true;
		}).ToList();

		return count;
	}
}
