namespace StockManagement.Import.Core.Contracts;


/// <summary>
/// An import failure for one row of a source file
/// </summary>
/// <param name="Row">1-based row number, as the user sees it in the source file</param>
/// <param name="Message">Localized failure reason</param>
public sealed record ImportRowError(int Row, string Message);
