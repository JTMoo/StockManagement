using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core;


/// <summary>
/// <see cref="IImportBatchService"/>: parses and de-duplicates through the matching <see cref="IImportTargetHandler"/>, everything else target-agnostic
/// </summary>
internal sealed class ImportBatchService : IImportBatchService
{
	private readonly IReadOnlyDictionary<ImportTarget, IImportTargetHandler> _handlers;
	private readonly IImportBatchServiceProvider _importBatchServiceProvider;


	public ImportBatchService(IEnumerable<IImportTargetHandler> handlers, IImportBatchServiceProvider importBatchServiceProvider)
	{
		_handlers = handlers.ToDictionary(handler => handler.Target);
		_importBatchServiceProvider = importBatchServiceProvider;
	}


	public IReadOnlyList<ImportField> GetFields(ImportTarget target) => this.GetHandler(target).GetFields();

	public Task<(string SheetName, IReadOnlyList<DetectedColumn> Columns)> DetectColumnsAsync(ImportTarget target, Stream excelFile, CancellationToken cancellationToken = default) =>
		this.GetHandler(target).DetectColumnsAsync(excelFile, cancellationToken);

	public async Task<ImportBatch> PreviewAsync(ImportTarget target, string fileName, Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken cancellationToken = default)
	{
		var handler = this.GetHandler(target);
		var (sheetName, candidates, parseErrors) = await handler.ParseAsync(excelFile, columnMapping, cancellationToken);

		var split = await handler.SplitDuplicatesAsync([.. candidates.Select(candidate => candidate.Candidate)], cancellationToken);
		var uniqueCandidates = new HashSet<object>(split.Unique, ReferenceEqualityComparer.Instance);

		List<ImportBatchRow> rows = [.. candidates.Select(candidate => new ImportBatchRow(
				candidate.Row,
				uniqueCandidates.Contains(candidate.Candidate) ? ImportRowStatus.Ready : ImportRowStatus.Duplicate,
				null,
				handler.SerializeCandidate(candidate.Candidate)))];
		rows.AddRange(parseErrors.Select(error => new ImportBatchRow(error.Row, ImportRowStatus.Error, error.Message, null)));
		rows.Sort((left, right) => left.RowNumber.CompareTo(right.RowNumber));

		var batch = new ImportBatch(target, fileName, sheetName, rows);
		await _importBatchServiceProvider.AddImportBatchAsync(batch, cancellationToken);
		return batch;
	}

	public Task<ImportBatch?> GetAsync(string batchId, CancellationToken cancellationToken = default)
	{
		return _importBatchServiceProvider.GetImportBatchAsync(batchId, cancellationToken);
	}

	public async Task<ImportBatch> CommitAsync(string batchId, CancellationToken cancellationToken = default)
	{
		var batch = await this.GetExistingBatchAsync(batchId, cancellationToken);
		var handler = this.GetHandler(batch.Target);

		var candidates = batch.ReadyRows.Select(row => handler.DeserializeCandidate(row.PayloadJson!)).ToList();
		var importedIds = await handler.CommitAsync(candidates, cancellationToken);

		batch.MarkCommitted(importedIds);
		await _importBatchServiceProvider.UpdateImportBatchAsync(batch, cancellationToken);
		return batch;
	}

	public async Task<ImportBatch> UndoAsync(string batchId, CancellationToken cancellationToken = default)
	{
		var batch = await this.GetExistingBatchAsync(batchId, cancellationToken);
		var handler = this.GetHandler(batch.Target);

		var committed = batch.ReadyRows
			.Where(row => row.ImportedEntityId is not null)
			.Select(row => (row.ImportedEntityId!, handler.DeserializeCandidate(row.PayloadJson!)))
			.ToList();
		await handler.UndoAsync(committed, cancellationToken);

		batch.MarkUndone();
		await _importBatchServiceProvider.UpdateImportBatchAsync(batch, cancellationToken);
		return batch;
	}

	private async Task<ImportBatch> GetExistingBatchAsync(string batchId, CancellationToken cancellationToken)
	{
		return await _importBatchServiceProvider.GetImportBatchAsync(batchId, cancellationToken) ?? throw new ImportBatchNotFoundException();
	}

	private IImportTargetHandler GetHandler(ImportTarget target)
	{
		return _handlers.TryGetValue(target, out var handler) ? handler : throw new NotSupportedException($"No import handler registered for {target}.");
	}
}
