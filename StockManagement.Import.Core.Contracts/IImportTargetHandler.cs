using StockManagement.Kernel.Model.Types;

namespace StockManagement.Import.Core.Contracts;


/// <summary>
/// Parses, de-duplicates, commits and undoes import candidates for one <see cref="ImportTarget"/>
/// </summary>
/// <remarks>Candidates are boxed as <see cref="object"/> so <see cref="IImportBatchService"/> can stay target-agnostic; each implementation only ever sees its own entity type.</remarks>
public interface IImportTargetHandler
{
	public ImportTarget Target { get; }

	/// <summary>
	/// Reads candidates from the first worksheet of an Excel file, each with its 1-based source row number
	/// </summary>
	public Task<(string SheetName, IReadOnlyList<(int Row, object Candidate)> Candidates, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default);

	/// <summary>
	/// Splits candidates into ones whose key is new and ones that would clash with an existing record or an earlier candidate
	/// </summary>
	public Task<DuplicateFilterResult<object>> SplitDuplicatesAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores the given candidates
	/// </summary>
	/// <returns>The id of the stored entity for each candidate, same order</returns>
	public Task<IReadOnlyList<string>> CommitAsync(IReadOnlyList<object> candidates, CancellationToken cancellationToken = default);

	/// <summary>
	/// Reverses what a commit did, per committed row
	/// </summary>
	/// <remarks>Most handlers created an entity per row and just delete it by <c>EntityId</c>; a handler that mutated an existing entity instead (no new id) reads what it needs back off <c>Candidate</c>.</remarks>
	public Task UndoAsync(IReadOnlyList<(string EntityId, object Candidate)> entities, CancellationToken cancellationToken = default);

	/// <summary>
	/// Serializes a candidate for storage on its <see cref="Kernel.Model.ImportBatchRow"/>
	/// </summary>
	public string SerializeCandidate(object candidate);

	/// <summary>
	/// Reverses <see cref="SerializeCandidate"/>, to rebuild candidates for a stored batch's commit
	/// </summary>
	public object DeserializeCandidate(string json);
}
