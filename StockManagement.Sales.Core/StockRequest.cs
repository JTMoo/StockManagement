namespace StockManagement.Sales.Core;


/// <summary>
/// Units of an article a sale wants to take, next to the units currently in stock.
/// </summary>
internal readonly record struct StockRequest(string Code, string Name, int Requested, int InStock);
