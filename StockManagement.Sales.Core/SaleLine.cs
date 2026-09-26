namespace StockManagement.Sales.Core;


/// <summary>
/// One position of a sale: how many units of which article at which unit price.
/// </summary>
internal readonly record struct SaleLine(string Code, string Name, int Quantity, decimal UnitPrice);
