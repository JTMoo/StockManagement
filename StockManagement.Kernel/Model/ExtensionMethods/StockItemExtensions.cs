namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class StockItemExtensions
{
	/// <summary>
	/// Sale price derived from purchase data: <c>(purchasePrice * exchangeRate + additionalPurchaseCost) * factor</c>.
	/// </summary>
	/// <remarks>Rounds to <paramref name="currencyDecimalDigits"/> using <see cref="MidpointRounding.AwayFromZero"/>, same as <c>InvoiceCalculator</c> (ADR-0014).</remarks>
	public static decimal CalculateSalePrice(decimal purchasePrice, decimal purchaseExchangeRate, decimal additionalPurchaseCost, decimal factor, int currencyDecimalDigits)
	{
		var cost = (purchasePrice * purchaseExchangeRate) + additionalPurchaseCost;
		return Math.Round(cost * factor, currencyDecimalDigits, MidpointRounding.AwayFromZero);
	}

	/// <summary>
	/// Distinct manufacturer names in use, sorted.
	/// </summary>
	/// <remarks>Empty names are skipped; names differing only in case count once (first spelling wins).</remarks>
	public static IReadOnlyList<string> GetManufacturers(this IEnumerable<StockItem> stockItems)
	{
		if (stockItems == null) return [];

		return stockItems
			.Select(stockItem => stockItem.Manufacturer)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Order(StringComparer.OrdinalIgnoreCase)
			.ToList();
	}
}
