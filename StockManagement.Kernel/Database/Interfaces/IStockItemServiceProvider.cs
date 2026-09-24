using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IStockItemServiceProvider
{
	public Task<StockItem> GetStockItemAsync(string code);
	public Task<StockItem> GetStockItemByIdAsync(string id);
	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync();

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateStockItemAsync(StockItem stockItem);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteStockItemAsync(StockItem stockItem);
	public Task AddStockItemAsync(StockItem stockItem);
	public Task AddManyStockItemsAsync(IList<StockItem> stockItem);
}
