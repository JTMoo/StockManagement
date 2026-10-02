namespace StockManagement.Sales.Core;


/// <summary>
/// Money rules for invoices. Amounts round to the company's configured currency digits (0 for a zero-decimal currency like PYG).
/// </summary>
internal static class InvoiceCalculator
{
	/// <summary>
	/// Sums quantity times unit price, with each unit price rounded to <paramref name="currencyDecimalDigits"/> first.
	/// </summary>
	/// <remarks>Rounding uses <see cref="MidpointRounding.AwayFromZero"/> (commercial rounding).</remarks>
	public static decimal CalculateTotal(IEnumerable<SaleLine> lines, int currencyDecimalDigits)
	{
		if (lines == null) return 0;

		return lines.Sum(line => line.Quantity * Round(line.UnitPrice, currencyDecimalDigits));
	}

	/// <summary>
	/// VAT contained in the lines' gross totals, grouped by each line's own rate, rounded once at the end.
	/// </summary>
	/// <remarks>Prices include VAT: for a line rate of 10, the VAT share of that line's gross total is 10/110.</remarks>
	public static decimal CalculateTax(IEnumerable<SaleLine> lines, int currencyDecimalDigits)
	{
		if (lines == null) return 0;

		var tax = lines
			.GroupBy(line => line.VatRatePercent)
			.Sum(group =>
			{
				var groupTotal = group.Sum(line => line.Quantity * Round(line.UnitPrice, currencyDecimalDigits));
				return groupTotal * group.Key / (100 + group.Key);
			});

		return Round(tax, currencyDecimalDigits);
	}

	public static DateTime CalculateExpirationDate(DateTime invoiceDate, int paymentTermInDays)
	{
		return invoiceDate.AddDays(paymentTermInDays);
	}

	private static decimal Round(decimal value, int digits)
	{
		return Math.Round(value, digits, MidpointRounding.AwayFromZero);
	}
}
