namespace StockManagement.Import.Core.Contracts;


/// <summary>
/// Import candidates split into records that can be inserted and records that would clash
/// </summary>
/// <param name="Unique">Candidates whose key is new</param>
/// <param name="Duplicates">Candidates whose key is already stored or repeated in the import</param>
public sealed record DuplicateFilterResult<T>(IReadOnlyList<T> Unique, IReadOnlyList<T> Duplicates);
