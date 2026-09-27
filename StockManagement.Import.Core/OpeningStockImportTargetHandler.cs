using System.Text.Json;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core;


/// <summary>
/// <see cref="IImportTargetHandler"/> for opening stock (ADR-0019): checks units into an already-existing <see cref="StockItem"/>, matched by <see cref="OpeningStockRow.Code"/>; nothing new is created
/// </summary>
/// <remarks>A code repeated in the file, or matching no existing stock item, lands in the same <see cref="DuplicateFilterResult{T}.Duplicates"/> bucket (ADR-0019 trade-off).</remarks>
internal sealed class OpeningStockImportTargetHandler(IStockItemServiceProvider stockItemServiceProvider) : IImportTargetHandler
{
	private const string CheckInReason = "Opening stock import";

	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	public ImportTarget Target => ImportTarget.OpeningStock;


	public async Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<OpeningStockRow>.ParseAsync(excelFile, cancellationToken);
		return (sheetName, [.. items.Select(item => (item.Row, (object)item.Item))], errors);
	}

	public async Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var existingCodes = (await _stockItemServiceProvider.GetAllStockItemsAsync())
			.Select(item => item.Code)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<object> unique = [];
		List<object> duplicates = [];
		foreach (var candidate in candidates)
		{
			var code = ((OpeningStockRow)candidate).Code;
			(existingCodes.Contains(code) && seenCodes.Add(code) ? unique : duplicates).Add(candidate);
		}

		return new(unique, duplicates);
	}

	public async Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default)
	{
		List<string> stockItemIds = [];
		foreach (var row in candidates.Cast<OpeningStockRow>())
		{
			var item = await _stockItemServiceProvider.GetStockItemAsync(row.Code);
			await _stockItemServiceProvider.CheckInStockItemAsync(item, row.Amount, CheckInReason);
			stockItemIds.Add(item.Id);
		}

		return stockItemIds;
	}

	public async Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default)
	{
		foreach (var (id, candidate) in entities)
		{
			var row = (OpeningStockRow)candidate;
			if (await _stockItemServiceProvider.GetStockItemByIdAsync(id) is StockItem item)
			{
				await _stockItemServiceProvider.TryCheckOutStockItemAsync(item, row.Amount, $"{CheckInReason} (undo)");
			}
		}
	}

	public string SerializeCandidate(object candidate) => JsonSerializer.Serialize((OpeningStockRow)candidate);

	public object DeserializeCandidate(string json) => JsonSerializer.Deserialize<OpeningStockRow>(json)!;
}
