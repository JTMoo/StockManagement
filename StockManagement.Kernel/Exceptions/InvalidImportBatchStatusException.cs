using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Exceptions;


public class InvalidImportBatchStatusException(ImportBatchStatus actual, ImportBatchStatus expected) : Exception($"Batch is {actual}, not {expected}.")
{
	public ImportBatchStatus Actual { get; } = actual;

	public ImportBatchStatus Expected { get; } = expected;
}
