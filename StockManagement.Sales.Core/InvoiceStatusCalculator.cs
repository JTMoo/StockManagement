using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Sales.Core;


/// <summary>
/// Payment status of an invoice, derived from its <see cref="Invoice.Payments"/>; never stored.
/// </summary>
internal static class InvoiceStatusCalculator
{
	public static decimal AmountPaid(Invoice invoice)
	{
		return (invoice.Payments ?? []).Sum(payment => payment.Amount);
	}

	public static decimal AmountDue(Invoice invoice)
	{
		return invoice.Total - AmountPaid(invoice);
	}

	/// <remarks><see cref="InvoiceStatus.Overdue"/> takes priority over <see cref="InvoiceStatus.PartiallyPaid"/>.</remarks>
	public static InvoiceStatus GetStatus(Invoice invoice, DateTime asOf)
	{
		if (AmountDue(invoice) <= 0) return InvoiceStatus.Paid;
		if (asOf > invoice.ExpirationDate) return InvoiceStatus.Overdue;

		return AmountPaid(invoice) > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Open;
	}
}
