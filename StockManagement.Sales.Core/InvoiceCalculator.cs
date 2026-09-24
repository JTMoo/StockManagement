namespace StockManagement.Sales.Core;


/// <summary>
/// Money rules for invoices. Amounts are whole currency units (no minor units).
/// </summary>
internal static class InvoiceCalculator
{
	public const int FirstInvoiceNumber = 1;

	/// <summary>
	/// Prices include VAT at 10 %, so the VAT share of a gross amount is 10/110 = 1/11.
	/// </summary>
	public const int TaxDivisor = 11;
	public const int PaymentTermInDays = 30;


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
	public static long CalculateTax(long total)
	{
		return (long)Math.Round((double)total / TaxDivisor);
	}

	public static DateTime CalculateExpirationDate(DateTime invoiceDate)
	{
		return invoiceDate.AddDays(PaymentTermInDays);
	}
}
