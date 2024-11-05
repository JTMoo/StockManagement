namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class IEnumerableExtensions
{
	public static List<ShoppingCartItem> ConvertToShoppingCartList(this IEnumerable<StockItem> items)
	{
		var cartItems = new List<ShoppingCartItem>();
		items.ToList().ForEach(item => cartItems.Add(new(item)));
		return cartItems;
	}
}
