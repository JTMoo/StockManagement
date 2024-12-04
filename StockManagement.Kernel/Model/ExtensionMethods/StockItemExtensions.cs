namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class StockItemExtensions
{
	public static void Register(this IEnumerable<StockItem> items)
	{
		if (items is null || !items.Any()) return;

		foreach (var item in items)
		{
			item.Register();
		}
	}
}
