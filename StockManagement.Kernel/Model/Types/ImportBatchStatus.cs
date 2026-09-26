namespace StockManagement.Kernel.Model.Types;


/// <summary>
/// Lifecycle of an <see cref="ImportBatch"/>
/// </summary>
public enum ImportBatchStatus
{
	/// <summary>
	/// Parsed and stored; nothing written to its target yet
	/// </summary>
	Previewed = 0,

	/// <summary>
	/// Its <see cref="ImportRowStatus.Ready"/> rows were written to the target
	/// </summary>
	Committed,

	/// <summary>
	/// A committed batch whose written rows were removed again
	/// </summary>
	Undone
}
