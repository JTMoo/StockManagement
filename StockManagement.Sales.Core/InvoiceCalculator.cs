namespace StockManagement.Sales.Core;


/// <summary>
/// Money rules for invoices. Amounts are whole currency units (no minor units).
/// </summary>
internal static class InvoiceCalculator
{
	/// <summary>
	/// Sums quantity times unit price, with each unit price rounded to a whole unit first.
	/// </summary>
	/// <remarks>Rounding uses <see cref="Math.Round(double)"/> (half to even), as the GUI always did.</remarks>
	public static long CalculateTotal(IEnumerable<SaleLine> lines)
	{
		if (lines == null) return 0;

		return lines.Sum(line => line.Quantity * Convert.ToInt64(Math.Round(line.UnitPrice, 0)));
	}

	/// <summary>
	/// VAT contained in a gross <paramref name="total"/>, rounded to a whole unit.
	/// </summary>
	/// <remarks>Prices include VAT: for a <paramref name="vatRatePercent"/> of 10, the VAT share of the gross total is 10/110.</remarks>
	public static long CalculateTax(long total, decimal vatRatePercent)
	{
		return (long)Math.Round((double)total * (double)vatRatePercent / (100 + (double)vatRatePercent));
	}

	public static DateTime CalculateExpirationDate(DateTime invoiceDate, int paymentTermInDays)
	{
		return invoiceDate.AddDays(paymentTermInDays);
	}
}
