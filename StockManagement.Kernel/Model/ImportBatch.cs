using StockManagement.Kernel.Database;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// <summary>
/// One upload parsed into rows, previewed, then committed to <see cref="Target"/> or undone
/// </summary>
public class ImportBatch : BaseDocument
{
	/// <summary>
	/// For EF Core materialization
	/// </summary>
	private ImportBatch()
	{
	}

	public ImportBatch(ImportTarget target, string fileName, string sheetName, IEnumerable<ImportBatchRow> rows)
	{
		this.Target = target;
		this.FileName = fileName;
		this.SheetName = sheetName;
		this.Status = ImportBatchStatus.Previewed;
		this.CreatedAtUtc = DateTime.UtcNow;
		this.Rows = [.. rows];
	}

	public ImportTarget Target { get; private set; }

	public string FileName { get; private set; } = string.Empty;

	public string SheetName { get; private set; } = string.Empty;

	public ImportBatchStatus Status { get; private set; }

	public DateTime CreatedAtUtc { get; private set; }

	public DateTime? CommittedAtUtc { get; private set; }

	public DateTime? UndoneAtUtc { get; private set; }

	public List<ImportBatchRow> Rows { get; private set; } = [];

	/// <remarks>Ordered by <see cref="ImportBatchRow.RowNumber"/> explicitly: reloaded from storage, <see cref="Rows"/> is not guaranteed to keep source-row order.</remarks>
	public IReadOnlyList<ImportBatchRow> ReadyRows => [.. this.Rows.Where(row => row.Status == ImportRowStatus.Ready).OrderBy(row => row.RowNumber)];

	/// <exception cref="InvalidImportBatchStatusException">Not <see cref="ImportBatchStatus.Previewed"/></exception>
	/// <exception cref="ArgumentException"><paramref name="importedEntityIds"/> does not have one id per <see cref="ReadyRows"/></exception>
	public void MarkCommitted(IReadOnlyList<string> importedEntityIds)
	{
		if (this.Status != ImportBatchStatus.Previewed) throw new InvalidImportBatchStatusException(this.Status, ImportBatchStatus.Previewed);

		var readyRows = this.ReadyRows;
		if (importedEntityIds.Count != readyRows.Count) throw new ArgumentException("One id per ready row is required.", nameof(importedEntityIds));

		for (var i = 0; i < readyRows.Count; i++) readyRows[i].MarkImported(importedEntityIds[i]);

		this.Status = ImportBatchStatus.Committed;
		this.CommittedAtUtc = DateTime.UtcNow;
	}

	/// <exception cref="InvalidImportBatchStatusException">Not <see cref="ImportBatchStatus.Committed"/></exception>
	public void MarkUndone()
	{
		if (this.Status != ImportBatchStatus.Committed) throw new InvalidImportBatchStatusException(this.Status, ImportBatchStatus.Committed);

		foreach (var row in this.Rows) row.MarkUndone();

		this.Status = ImportBatchStatus.Undone;
		this.UndoneAtUtc = DateTime.UtcNow;
	}
}
