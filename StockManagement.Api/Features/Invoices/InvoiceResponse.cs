using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Invoices;


/// <summary>
/// Stored invoice; amounts in whole currency units
/// </summary>
public sealed record InvoiceResponse(int Number, DateTime Date, DateTime ExpirationDate, long Total, long Tax, SaleCondition SaleCondition, int CustomerId, string CustomerName, IReadOnlyList<InvoiceLineResponse> Lines)
{
	public static InvoiceResponse From(Invoice invoice)
	{
		var lines = (invoice.Items ?? []).Select(item => new InvoiceLineResponse(item.StockItem.Code, item.StockItem.Name, item.Amount, (decimal)item.StockItem.Price)).ToList();
		var customerName = invoice.Customer is Customer customer ? string.Join(" ", customer.Name, customer.Lastname).Trim() : "";
		return new(invoice.Number, invoice.Date, invoice.ExpirationDate, invoice.Total, invoice.Tax, invoice.SaleCondition, invoice.Customer?.CustomerId ?? 0, customerName, lines);
	}
}


public sealed record InvoiceLineResponse(string Code, string Name, int Amount, decimal UnitPrice);
