using StockManagement.Import.Core.Contracts;

namespace StockManagement.Import.Core;


internal static class DuplicateFilter
{
	/// <summary>
	/// Splits import <paramref name="candidates"/> into records that can be inserted and records that would clash
	/// </summary>
	/// <remarks>
	/// A candidate is a duplicate when its key already exists in <paramref name="existing"/> or an earlier candidate has it.
	/// The first candidate with a new key wins. The order of <paramref name="candidates"/> is kept in both lists.
	/// </remarks>
	public static DuplicateFilterResult<T> Split<T, TKey>(IEnumerable<T> candidates, IEnumerable<T> existing, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
		where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(keySelector);
		if (candidates == null) return new([], []);

		comparer ??= EqualityComparer<TKey>.Default;
		var takenKeys = new HashSet<TKey>((existing ?? []).Select(keySelector), comparer);

		List<T> unique = [];
		List<T> duplicates = [];
		foreach (var candidate in candidates)
		{
			var isNew = takenKeys.Add(keySelector(candidate));
			(isNew ? unique : duplicates).Add(candidate);
		}

		return new(unique, duplicates);
	}
}
