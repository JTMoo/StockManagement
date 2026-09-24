namespace StockManagement.Kernel.Database;


/// <summary>
/// One page of a filtered query
/// </summary>
/// <param name="TotalCount">Matching rows across every page, not just <paramref name="Items"/></param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
