namespace StockManagement.Kernel.Util;


/// <summary>
/// Normalizes and verifies a Paraguayan RUC (SET modulo-11 check digit)
/// </summary>
public static class RucValidator
{
	/// <summary>
	/// Strips formatting from <paramref name="ruc"/>, verifies its check digit and, on success, writes <c>"digits-checkDigit"</c> to <paramref name="normalized"/>
	/// </summary>
	/// <remarks>Accepts both <c>"1234567-8"</c> and <c>"12345678"</c>; anything but digits is ignored.</remarks>
	public static bool TryNormalize(string? ruc, out string normalized)
	{
		normalized = string.Empty;
		if (string.IsNullOrWhiteSpace(ruc)) return false;

		var digits = new string([.. ruc.Where(char.IsDigit)]);
		if (digits.Length < 2) return false;

		var baseDigits = digits[..^1];
		var checkDigit = digits[^1] - '0';
		if (checkDigit != Mod11.ComputeCheckDigit(baseDigits)) return false;

		normalized = $"{baseDigits}-{checkDigit}";
		return true;
	}
}
