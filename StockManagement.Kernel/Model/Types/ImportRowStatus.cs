namespace StockManagement.Kernel.Model.Types;


/// <summary>
/// Outcome of parsing and de-duplicating one source row
/// </summary>
public enum ImportRowStatus
{
	/// <summary>
	/// Parsed and not a duplicate; committed when the batch is committed
	/// </summary>
	Ready = 0,

	/// <summary>
	/// Parsed, but its key already exists or repeats an earlier row; skipped on commit
	/// </summary>
	Duplicate,

	/// <summary>
	/// Failed to parse; skipped on commit
	/// </summary>
	Error
}
