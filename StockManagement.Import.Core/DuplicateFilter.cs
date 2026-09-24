using StockManagement.Import.Core.Contracts;

namespace StockManagement.Import.Core;


internal static class DuplicateFilter
{
	/// <summary>
	/// Splits import <paramref name="candidates"/> into records that can be inserted and records that would clash
	/// </summary>
	/// <remarks>
	/// A candidate is a duplicate when its key already exists in <paramref name="existing"/>, or when the key appears
	/// more than once among the candidates; then every copy is a duplicate, because there is no rule yet for which copy wins.
	/// The order of <paramref name="candidates"/> is kept in both lists.
	/// </remarks>
	public static DuplicateFilterResult<T> Split<T, TKey>(IEnumerable<T> candidates, IEnumerable<T> existing, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
		where TKey : notnull
	{
		ArgumentNullException.ThrowIfNull(keySelector);
		if (candidates == null) return new([], []);

		comparer ??= EqualityComparer<TKey>.Default;
		var candidateList = candidates.ToList();
		var existingKeys = new HashSet<TKey>((existing ?? []).Select(keySelector), comparer);
		var keyCounts = candidateList
			.GroupBy(keySelector, comparer)
			.ToDictionary(group => group.Key, group => group.Count(), comparer);

		List<T> unique = [];
		List<T> duplicates = [];
		foreach (var candidate in candidateList)
		{
			var key = keySelector(candidate);
			var isDuplicate = existingKeys.Contains(key) || keyCounts[key] > 1;
			(isDuplicate ? duplicates : unique).Add(candidate);
		}

		return new(unique, duplicates);
	}
}
