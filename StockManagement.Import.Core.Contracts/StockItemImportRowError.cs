namespace StockManagement.Import.Core.Contracts;


/// <param name="Row">1-based Excel row number, as the user sees it in the file</param>
/// <param name="Message">Localized failure reason</param>
public sealed record StockItemImportRowError(int Row, string Message);
