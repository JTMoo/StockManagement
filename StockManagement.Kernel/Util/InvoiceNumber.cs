namespace StockManagement.Kernel.Util;


/// <summary>
/// DNIT composite invoice number: establishment-pointOfSale-sequence (e.g. "001-001-0000001")
/// </summary>
public static class InvoiceNumber
{
	public static string Format(string establishmentCode, string pointOfSaleCode, int sequence)
	{
		return $"{establishmentCode}-{pointOfSaleCode}-{sequence:D7}";
	}

	/// <summary>
	/// Sequence component of <paramref name="number"/> when it belongs to <paramref name="establishmentCode"/>/<paramref name="pointOfSaleCode"/>; <see langword="null"/> otherwise (a different scope, or not a composite number)
	/// </summary>
	public static int? TryParseSequence(string number, string establishmentCode, string pointOfSaleCode)
	{
		if (string.IsNullOrEmpty(number)) return null;

		var prefix = $"{establishmentCode}-{pointOfSaleCode}-";
		if (!number.StartsWith(prefix, StringComparison.Ordinal)) return null;

		return int.TryParse(number.AsSpan(prefix.Length), out var sequence) ? sequence : null;
	}
}
