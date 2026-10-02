using System.Globalization;
using System.Text;

namespace StockManagement.Kernel.Util;


/// <summary>
/// Normalizes a customer name for duplicate matching: accents, case, word order and company suffixes stop mattering
/// </summary>
public static class NameNormalizer
{
	private static readonly HashSet<string> CompanySuffixes = new(StringComparer.Ordinal)
	{
		"sa", "srl", "sac", "eirl", "ltda", "eas",
	};


	/// <summary>
	/// Returns a key that is equal for names differing only by accents, case, punctuation, word order or a trailing company suffix
	/// </summary>
	/// <remarks>Empty for blank input; callers must not treat two blanks as a match (same rule as <see cref="RucValidator"/>'s caller).</remarks>
	public static string Normalize(string? name)
	{
		if (string.IsNullOrWhiteSpace(name)) return string.Empty;

		var words = StripDiacritics(name)
			.ToLowerInvariant()
			.Replace(".", string.Empty)
			.Split(new[] { ' ', ',', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
			.Where(word => !CompanySuffixes.Contains(word))
			.OrderBy(word => word, StringComparer.Ordinal);

		return string.Join(' ', words);
	}

	private static string StripDiacritics(string value)
	{
		var decomposed = value.Normalize(NormalizationForm.FormD);
		var builder = new StringBuilder(decomposed.Length);
		foreach (var c in decomposed)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) builder.Append(c);
		}

		return builder.ToString().Normalize(NormalizationForm.FormC);
	}
}
