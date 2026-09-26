using System.Text.Json;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core;


/// <summary>
/// <see cref="IImportTargetHandler"/> for <see cref="StockItem"/>, reusing <see cref="IStockItemImportService"/>'s duplicate-split and insert logic
/// </summary>
internal sealed class StockItemImportTargetHandler(IStockItemImportService importService, IStockItemServiceProvider stockItemServiceProvider) : IImportTargetHandler
{
	private readonly IStockItemImportService _importService = importService;
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public ImportTarget Target => ImportTarget.StockItems;


	public async Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<StockItem>.ParseAsync(excelFile, cancellationToken);
		return (sheetName, [.. items.Select(item => (item.Row, (object)item.Item))], errors);
	}

	public async Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		var split = await _importService.SplitDuplicatesAsync(candidates.Cast<StockItem>(), cancellationToken);
		return new([.. split.Unique.Cast<object>()], [.. split.Duplicates.Cast<object>()]);
	}

	public async Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		var items = candidates.Cast<StockItem>().ToList();
		await _importService.ImportAsync(items, cancellationToken);
		return [.. items.Select(item => item.Id)];
	}

	public async Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default)
	{
		foreach (var (id, _) in entities)
		{
			if (await _stockItemServiceProvider.GetStockItemByIdAsync(id) is StockItem item)
			{
				await _stockItemServiceProvider.DeleteStockItemAsync(item);
			}
		}
	}

	public string SerializeCandidate(object candidate) => JsonSerializer.Serialize((StockItem)candidate);

	public object DeserializeCandidate(string json) => JsonSerializer.Deserialize<StockItem>(json)!;
}
