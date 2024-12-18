using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IStockItemServiceProvider
{
	public Task<StockItem> GetStockItemAsync(string code);
	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync();
	public Task<ReplaceOneResult> UpdateStockItemAsync(StockItem stockItem);
	public Task<DeleteResult> DeleteStockItemAsync(StockItem stockItem);
	public Task AddStockItemAsync(StockItem stockItem);
	public Task AddManyStockItemsAsync(IList<StockItem> stockItem);
	public Task<int> RemoveDuplicates(List<StockItem> stockItems);
}
