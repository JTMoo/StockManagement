using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IStockItemServiceProvider
{
	public Task<StockItem> GetStockItemAsync<T>(string code);
	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync();
	public Task<ReplaceOneResult> UpdateStockItemAsync<T>(StockItem stockItem);
	public Task<DeleteResult> DeleteStockItemAsync<T>(StockItem stockItem);
	public Task AddStockItemAsync<T>(StockItem stockItem);
}
