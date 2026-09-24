namespace StockManagement.Import.Core.Contracts;


public sealed record DuplicateFilterResult<T>(IReadOnlyList<T> Unique, IReadOnlyList<T> Duplicates);
