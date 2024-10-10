using System.Text.RegularExpressions;

namespace StockManagement.Kernel.Model.ExtensionMethods;


public static partial class StringExtensions
{
	public static string ReplaceLineBreakWithWhitespace(this string input)
	{
		return LineBreakDetection().Replace(input, " ");
	}

	[GeneratedRegex(@"\t|\n|\r")]
	private static partial Regex LineBreakDetection();
}
