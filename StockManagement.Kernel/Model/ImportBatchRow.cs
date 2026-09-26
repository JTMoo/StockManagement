using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


/// <summary>
/// One source row of an <see cref="ImportBatch"/>
/// </summary>
public sealed class ImportBatchRow
{
	/// <summary>
	/// For EF Core materialization
	/// </summary>
	private ImportBatchRow()
	{
	}

	public ImportBatchRow(int rowNumber, ImportRowStatus status, string? errorMessage, string? payloadJson)
	{
		this.RowNumber = rowNumber;
		this.Status = status;
		this.ErrorMessage = errorMessage;
		this.PayloadJson = payloadJson;
	}

	/// <summary>
	/// 1-based row number, as the user sees it in the source file
	/// </summary>
	public int RowNumber { get; private set; }

	public ImportRowStatus Status { get; private set; }

	/// <summary>
	/// Localized failure reason; set only when <see cref="Status"/> is <see cref="ImportRowStatus.Error"/>
	/// </summary>
	public string? ErrorMessage { get; private set; }

	/// <summary>
	/// The parsed candidate entity, serialized; set for <see cref="ImportRowStatus.Ready"/> and <see cref="ImportRowStatus.Duplicate"/> rows
	/// </summary>
	public string? PayloadJson { get; private set; }

	/// <summary>
	/// Id of the entity this row created, once the batch is committed
	/// </summary>
	public string? ImportedEntityId { get; private set; }

	public void MarkImported(string entityId)
	{
		this.ImportedEntityId = entityId;
	}

	public void MarkUndone()
	{
		this.ImportedEntityId = null;
	}
}
