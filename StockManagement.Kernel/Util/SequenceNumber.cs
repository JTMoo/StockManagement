namespace StockManagement.Kernel.Util;


/// <summary>
/// Sequential numbers such as invoice numbers and customer ids.
/// </summary>
public static class SequenceNumber
{
	/// <summary>
	/// Next number after the highest one in use, or <paramref name="first"/> when none is in use yet.
	/// </summary>
	public static int Next(IEnumerable<int> numbersInUse, int first)
	{
		if (numbersInUse == null) return first;

		var highest = numbersInUse.DefaultIfEmpty(first - 1).Max();
		return highest + 1;
	}
}
