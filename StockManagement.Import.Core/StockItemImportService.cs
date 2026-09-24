using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Import.Core;


internal class StockItemImportService(IStockItemServiceProvider stockItemServiceProvider) : IStockItemImportService
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public async Task<DuplicateFilterResult<StockItem>> SplitDuplicatesAsync(IEnumerable<StockItem> candidates, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existing = await _stockItemServiceProvider.GetAllStockItemsAsync() ?? [];
		return DuplicateFilter.Split(candidates, existing, stockItem => stockItem.Code ?? string.Empty, StringComparer.Ordinal);
	}

	/// <remarks>An empty import is skipped, because Mongo rejects InsertMany with no documents.</remarks>
	public Task ImportAsync(IEnumerable<StockItem> stockItems, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		List<StockItem> items = [.. stockItems ?? []];
		if (items.Count == 0) return Task.CompletedTask;

		return _stockItemServiceProvider.AddManyStockItemsAsync(items);
	}
}
