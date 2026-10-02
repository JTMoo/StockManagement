using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IStockItemServiceProvider
{
	public Task<StockItem> GetStockItemAsync(string code, CancellationToken cancellationToken = default);
	public Task<StockItem> GetStockItemByIdAsync(string id, CancellationToken cancellationToken = default);
	public Task<IEnumerable<StockItem>> GetAllStockItemsAsync(CancellationToken cancellationToken = default);

	/// <returns>Items with <see cref="StockItem.MinimumStock"/> &gt; 0 and <see cref="StockItem.Amount"/> below it (#57)</returns>
	public Task<IEnumerable<StockItem>> GetStockItemsBelowMinimumAsync(CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default);
	public Task AddStockItemAsync(StockItem stockItem, CancellationToken cancellationToken = default);
	public Task AddManyStockItemsAsync(IList<StockItem> stockItem, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds units to stock, recording a <see cref="Transaction"/> with the given reason
	/// </summary>
	public Task CheckInStockItemAsync(StockItem stockItem, int amount, string reason, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes units from stock if enough are in stock, recording a <see cref="Transaction"/> with the given reason
	/// </summary>
	/// <returns>False when fewer than <paramref name="amount"/> units are in stock; nothing written then</returns>
	public Task<bool> TryCheckOutStockItemAsync(StockItem stockItem, int amount, string reason, CancellationToken cancellationToken = default);
}
