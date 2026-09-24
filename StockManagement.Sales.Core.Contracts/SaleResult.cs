namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Outcome of <see cref="ISaleService.CompleteSaleAsync"/>.
/// </summary>
/// <param name="UnavailableItems">Names of articles with too little stock; empty on success</param>
public sealed record SaleResult(IReadOnlyList<string> UnavailableItems)
{
	public static SaleResult Success { get; } = new([]);

	public bool Succeeded => this.UnavailableItems.Count == 0;
}
