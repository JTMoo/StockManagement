using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class StockItemServiceProvider(IDatabase database) : IStockItemServiceProvider
{
	private readonly IDatabase _database = database;


	public Task AddStockItemAsync<T>(StockItem stockItem)
	{
		// This is not ideal - the add process should fail if the number is taken!
		var stockItemExistsCheck = this.GetStockItemAsync<T>(stockItem.Code).ContinueWith(task => task.Result != null).Result;
		if (stockItemExistsCheck) throw new StockItemCodeAlreadyExistsException();

		var addTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount);
		_database.Add<Transaction>(addTransaction);
		return _database.Add<T>(stockItem);
	}

	public Task<DeleteResult> DeleteStockItemAsync<T>(StockItem stockItem)
	{
		var deleteTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1);
		_database.Add<Transaction>(deleteTransaction);
		return _database.Delete<T>(stockItem);
	}

	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
	{
		var machines = _database.GetAll<Machine>().ContinueWith(task => task.Result.Cast<StockItem>());
		var tires = _database.GetAll<Tire>().ContinueWith(task => task.Result.Cast<StockItem>());
		var spareParts = _database.GetAll<SparePart>().ContinueWith(task => task.Result.Cast<StockItem>());
		return machines.ContinueWith(task => task.Result.Concat(tires.Result).Concat(spareParts.Result));
	}

	public Task<StockItem> GetStockItemAsync<T>(string code)
	{
		return _database.GetOneAsync<T>(item => (item as StockItem).Code == code).ContinueWith(task => task.Result as StockItem);
	}

	public Task<ReplaceOneResult> UpdateStockItemAsync<T>(StockItem stockItem)
	{
		var item = this.GetStockItemAsync<T>(stockItem.Code).Result;
		if (item.Amount != stockItem.Amount)
		{
			var changeAmountTransaction = new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount - item.Amount);
			_database.Add<Transaction>(changeAmountTransaction);
		}

		return _database.Update<StockItem>(stockItem);
	}
}
