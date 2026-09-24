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

	/// <summary>
	/// Takes <paramref name="amount"/> units of <paramref name="code"/> out of stock in one atomic write
	/// </summary>
	/// <remarks>Records the change as <see cref="Transaction"/>. Stock never drops below 0.</remarks>
	/// <returns>Stock item after the change; <see langword="null"/> when unknown or too little stock (nothing written)</returns>
	public Task<StockItem?> TryTakeStockAsync(string code, int amount, CancellationToken cancellationToken = default);

	/// <summary>
	/// Puts <paramref name="amount"/> units of <paramref name="code"/> back into stock in one atomic write
	/// </summary>
	/// <remarks>Undo for <see cref="TryTakeStockAsync"/>. Records the change as <see cref="Transaction"/>.</remarks>
	public Task ReturnStockAsync(string code, int amount, CancellationToken cancellationToken = default);
}
