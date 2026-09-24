using StockManagement.Kernel.Model;

namespace StockManagement.Import.Core.Contracts;


public interface IStockItemImportService
{
	/// <summary>
	/// Separates imported articles whose code is new from those whose code is already stored or repeated in the import.
	/// </summary>
	public Task<DuplicateFilterResult<StockItem>> SplitDuplicatesAsync(IEnumerable<StockItem> candidates, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores the given articles in one call; an empty list writes nothing
	/// </summary>
	public Task ImportAsync(IEnumerable<StockItem> stockItems, CancellationToken cancellationToken = default);
}
