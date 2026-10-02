using System.Text;

namespace StockManagement.Kernel.Database;


/// <summary>
/// Opaque pagination cursor (ADR-0029): base64 of the last row's sort-column value(s) plus its Id, in order
/// </summary>
public static class Cursor
{
	private const char Separator = '\u0001';


	public static string Encode(params string[] parts)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(Separator, parts)));
	}

	/// <returns>The decoded parts, or <see langword="null"/> when <paramref name="cursor"/> is missing, malformed, or has the wrong number of parts</returns>
	public static string[]? TryDecode(string? cursor, int expectedParts)
	{
		if (string.IsNullOrEmpty(cursor)) return null;

		try
		{
			var parts = Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Split(Separator);
			return parts.Length == expectedParts ? parts : null;
		}
		catch (FormatException)
		{
			return null;
		}
	}
}
