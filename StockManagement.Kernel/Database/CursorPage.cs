namespace StockManagement.Kernel.Database;


/// <summary>
/// One page of a cursor-paginated list endpoint (ADR-0029)
/// </summary>
/// <param name="NextCursor"><see langword="null"/> when <paramref name="Items"/> is the last page</param>
public sealed record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
