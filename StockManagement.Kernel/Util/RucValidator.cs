namespace StockManagement.Kernel.Util;


/// <summary>
/// Normalizes and verifies a Paraguayan RUC (SET modulo-11 check digit)
/// </summary>
public static class RucValidator
{
	private const int BaseMax = 11;


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
		if (checkDigit != ComputeCheckDigit(baseDigits)) return false;

		normalized = $"{baseDigits}-{checkDigit}";
		return true;
	}

	private static int ComputeCheckDigit(string baseDigits)
	{
		var total = 0;
		var factor = 2;
		for (var i = baseDigits.Length - 1; i >= 0; i--)
		{
			total += (baseDigits[i] - '0') * factor;
			factor = factor == BaseMax ? 2 : factor + 1;
		}

		var remainder = total % BaseMax;
		return remainder <= 1 ? 0 : BaseMax - remainder;
	}
}
