using System.Collections.ObjectModel;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static class ObservableCollectionExtensions
{
	/// <summary>
	/// Equalizes the items of <paramref name="collection1"/> to the items of <paramref name="collection2"/>
	/// </summary>
	/// <typeparam name="T">Type of the items in the collections</typeparam>
	/// <param name="collection1"></param>
	/// <param name="collection2"></param>
	/// <returns>True, if the <paramref name="collection1"/> changed.</returns>
	public static bool EqualizeTo<T>(this ObservableCollection<T> collection1, IEnumerable<T> collection2)
	{
		if (collection1 == null || collection2 == null) return false;

		var itemsToRemove = collection1.Except(collection2).ToList();
		foreach (var item in itemsToRemove)
		{
			collection1.Remove(item);
		}

		var itemsToAdd = collection2.Except(collection1).ToList();
		foreach (var item in itemsToAdd)
		{
			collection1.Add(item);
		}

		return itemsToAdd.Count > 0 || itemsToRemove.Count > 0;
	}
}
