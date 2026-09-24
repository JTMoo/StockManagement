using System.Text.RegularExpressions;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static partial class StringExtensions
{
	public static string ReplaceLineBreakWithWhitespace(this string input)
	{
		return LineBreakDetection().Replace(input, " ");
	}

	/// <summary>
	/// Whether <paramref name="value"/> contains <paramref name="search"/>, ignoring case
	/// </summary>
	/// <remarks>An empty search matches everything. The search is plain text, not a pattern.</remarks>
	public static bool MatchesSearch(this string value, string search)
	{
		if (string.IsNullOrEmpty(search)) return true;
		return value?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false;
	}

	[GeneratedRegex(@"\t|\n|\r")]
	private static partial Regex LineBreakDetection(); 
}
