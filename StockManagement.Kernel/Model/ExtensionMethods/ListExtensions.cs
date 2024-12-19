namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class ListExtensions
{
	/// <summary>
	/// Equalizes the items of <paramref name="collection1"/> to the items of <paramref name="collection2"/>
	/// </summary>
	/// <typeparam name="T">Type of the items in the collections</typeparam>
	/// <param name="collection1"></param>
	/// <param name="collection2"></param>
	/// <returns>True, if the <paramref name="collection1"/> changed.</returns>
	public static bool EqualizeTo<T>(this List<T> collection1, IEnumerable<T> collection2)
	{
		if (collection1 == null || collection2 == null) return false;

		var itemsToRemove = collection1.Except(collection2).ToList();
		itemsToRemove.ForEach(item => collection1.Remove(item));

		var itemsToAdd = collection2.Except(collection1).ToList();
		itemsToAdd.ForEach(collection1.Add);

		return itemsToAdd.Count != 0 || itemsToRemove.Count != 0;
	}

	public static int RemoveAndCountDuplicates(this List<StockItem> stockItems, IEnumerable<StockItem> existingItems)
	{
		var count = 0;
		for (var index = stockItems.Count - 1; index >= 0; index--)
		{
			var currentItem = stockItems[index];
			if (!existingItems.Any(existingItem => currentItem.Code == existingItem.Code) && stockItems.Count(item => currentItem.Code == item.Code) == 1) continue;

			stockItems.Remove(currentItem);
			count++;
		}

		return count;
	}
}
