namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class StockItemExtensions
{
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
