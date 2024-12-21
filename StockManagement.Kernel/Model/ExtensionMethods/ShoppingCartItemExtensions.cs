using StockManagement.Kernel.Database.Interfaces;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class ShoppingCartItemExtensions
{
	public static async Task UpdateStockItems(this List<ShoppingCartItem> items, IStockItemServiceProvider serviceProvider)
	{
		if (items == null || items.Count == 0) return;
		foreach (var item in items)
		{
			if (item.Amount > item.StockItem.Amount) throw new ArgumentOutOfRangeException(item.StockItem.Name, Language.Resources.exceptionShoppingCartItemOutOfRange);
			
			item.StockItem.Amount -= item.Amount;
			await serviceProvider.UpdateStockItemAsync(item.StockItem);
		}
	}
}
