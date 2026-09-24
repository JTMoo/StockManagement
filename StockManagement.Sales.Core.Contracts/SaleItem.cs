namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Units of one article to sell
/// </summary>
/// <param name="Code">Article code</param>
/// <param name="Amount">Units, greater than 0</param>
public sealed record SaleItem(string Code, int Amount);
