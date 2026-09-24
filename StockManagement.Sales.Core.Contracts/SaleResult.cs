namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Outcome of <see cref="ISaleService.CompleteSaleAsync"/>.
/// </summary>
/// <param name="UnavailableItems">Names of articles with too little stock; empty on success</param>
public sealed record SaleResult(IReadOnlyList<string> UnavailableItems)
{
	/// <summary>
	/// Result of a sale that went through
	/// </summary>
	public static SaleResult Success { get; } = new([]);

	/// <summary>
	/// True when every article had enough stock and the invoice was stored
	/// </summary>
	public bool Succeeded => this.UnavailableItems.Count == 0;
}
