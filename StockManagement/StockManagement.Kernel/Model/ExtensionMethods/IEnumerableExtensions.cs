namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class IEnumerableExtensions
{
	public static List<ShoppingCartItem> ConvertToShoppingCartList(this IEnumerable<StockItem> items)
	{
		var cartItems = new List<ShoppingCartItem>();
		items.ToList().ForEach(item => cartItems.Add(new(item)));
		return cartItems;
	}

	public static IEnumerable<T> Where<T>(this IEnumerable<T> items, List<Func<T, bool>> filters)
	{
		return items.Where(item =>
		{
			var result = true;
			foreach (var filter in filters)
			{
				result &= filter(item);
			}
			return result;
		});
	}
}
