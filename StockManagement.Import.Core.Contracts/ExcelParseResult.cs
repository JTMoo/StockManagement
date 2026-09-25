using StockManagement.Kernel.Model;

namespace StockManagement.Import.Core.Contracts;


/// <param name="SheetName">Worksheet the items were read from</param>
/// <param name="Items">Rows that converted successfully</param>
/// <param name="Errors">Rows that failed to convert, one entry per row</param>
public sealed record ExcelParseResult(string SheetName, IReadOnlyList<StockItem> Items, IReadOnlyList<StockItemImportRowError> Errors);
