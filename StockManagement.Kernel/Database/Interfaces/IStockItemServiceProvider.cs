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

	/// <summary>
	/// Adds units to stock, recording a <see cref="Transaction"/> with the given reason
	/// </summary>
	public Task CheckInStockItemAsync(StockItem stockItem, int amount, string reason);

	/// <summary>
	/// Removes units from stock if enough are in stock, recording a <see cref="Transaction"/> with the given reason
	/// </summary>
	/// <returns>False when fewer than <paramref name="amount"/> units are in stock; nothing written then</returns>
	public Task<bool> TryCheckOutStockItemAsync(StockItem stockItem, int amount, string reason);
}
