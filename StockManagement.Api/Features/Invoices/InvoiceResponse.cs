using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Api.Features.Invoices;


/// <summary>
/// Stored invoice; amounts rounded to the company's configured currency digits
/// </summary>
public sealed record InvoiceResponse(string Number, DateTime Date, DateTime ExpirationDate, decimal Total, decimal Tax, decimal AmountPaid, decimal AmountDue, InvoiceStatus Status, SaleCondition SaleCondition, int CustomerId, string CustomerName, bool IsCancelled, IReadOnlyList<InvoiceLineResponse> Lines)
{
	public static InvoiceResponse From(Invoice invoice, IPaymentService paymentService)
	{
		var lines = (invoice.Items ?? []).Select(item => new InvoiceLineResponse(item.StockItem.Code, item.StockItem.Name, item.Amount, item.StockItem.Price)).ToList();
		var customerName = invoice.Customer is Customer customer ? string.Join(" ", customer.Name, customer.Lastname).Trim() : "";
		var now = DateTime.Now;
		return new(invoice.Number, invoice.Date, invoice.ExpirationDate, invoice.Total, invoice.Tax, paymentService.GetAmountPaid(invoice), paymentService.GetAmountDue(invoice), paymentService.GetStatus(invoice, now),
			invoice.SaleCondition, invoice.Customer?.CustomerId ?? 0, customerName, invoice.IsCancelled, lines);
	}
}


public sealed record InvoiceLineResponse(string Code, string Name, int Amount, decimal UnitPrice);
