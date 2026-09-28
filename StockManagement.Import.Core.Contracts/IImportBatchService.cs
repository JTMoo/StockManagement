using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core.Contracts;


/// <summary>
/// Generic upload → preview → commit → undo pipeline, target-specific work delegated to an <see cref="IImportTargetHandler"/>
/// </summary>
public interface IImportBatchService
{
	/// <summary>
	/// <paramref name="target"/>'s importable fields, for the web column-mapping UI
	/// </summary>
	public IReadOnlyList<ImportField> GetFields(ImportTarget target);

	/// <summary>
	/// Reads <paramref name="excelFile"/>'s header row, with each column's auto-matched field (or none)
	/// </summary>
	public Task<(string SheetName, IReadOnlyList<DetectedColumn> Columns)> DetectColumnsAsync(ImportTarget target, Stream excelFile, CancellationToken cancellationToken = default);

	/// <summary>
	/// Parses <paramref name="excelFile"/>, splits duplicates and stores the result as a new batch; nothing is written to <paramref name="target"/> yet
	/// </summary>
	/// <param name="columnMapping">Column number → <see cref="ImportField.Name"/>; <see langword="null"/> auto-matches headers by name instead</param>
	public Task<ImportBatch> PreviewAsync(ImportTarget target, string fileName, Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken cancellationToken = default);

	/// <returns><see langword="null"/> if no batch has that id</returns>
	public Task<ImportBatch?> GetAsync(string batchId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes a previewed batch's ready rows to its target
	/// </summary>
	/// <exception cref="Kernel.Exceptions.ImportBatchNotFoundException">No batch has that id</exception>
	/// <exception cref="Kernel.Exceptions.InvalidImportBatchStatusException">The batch is not <see cref="ImportBatchStatus.Previewed"/></exception>
	public Task<ImportBatch> CommitAsync(string batchId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the entities a commit created
	/// </summary>
	/// <exception cref="Kernel.Exceptions.ImportBatchNotFoundException">No batch has that id</exception>
	/// <exception cref="Kernel.Exceptions.InvalidImportBatchStatusException">The batch is not <see cref="ImportBatchStatus.Committed"/></exception>
	public Task<ImportBatch> UndoAsync(string batchId, CancellationToken cancellationToken = default);
}
