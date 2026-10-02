namespace StockManagement.Kernel.Util;


/// <summary>
/// SET módulo-11 check digit, shared by Paraguayan tax document identifiers (RUC, CDC).
/// </summary>
public static class Mod11
{
	private const int MaxFactor = 11;


	/// <summary>
	/// Check digit for <paramref name="digits"/>: factors 2-11 cycling from the rightmost digit, remainder of the weighted sum mod 11.
	/// </summary>
	public static int ComputeCheckDigit(string digits)
	{
		var total = 0;
		var factor = 2;
		for (var i = digits.Length - 1; i >= 0; i--)
		{
			total += (digits[i] - '0') * factor;
			factor = factor == MaxFactor ? 2 : factor + 1;
		}

		var remainder = total % MaxFactor;
		return remainder <= 1 ? 0 : MaxFactor - remainder;
	}
}
