using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Sales.Core.Contracts;


public interface IPaymentService
{
	/// <summary>
	/// Sum of every payment recorded against the invoice
	/// </summary>
	public decimal GetAmountPaid(Invoice invoice);

	/// <summary>
	/// <see cref="Invoice.Total"/> minus <see cref="GetAmountPaid"/>
	/// </summary>
	public decimal GetAmountDue(Invoice invoice);

	/// <summary>
	/// Open / partly paid / paid / overdue, derived from payments and <paramref name="asOf"/> vs. <see cref="Invoice.ExpirationDate"/>
	/// </summary>
	public InvoiceStatus GetStatus(Invoice invoice, DateTime asOf);

	/// <summary>
	/// Records a payment against the invoice; rounds <paramref name="amount"/> to the company's configured currency digits first
	/// </summary>
	/// <remarks>Rejected when the invoice does not exist, the amount is not positive, or it exceeds the invoice's amount due.</remarks>
	public Task<RecordPaymentResult> RecordPaymentAsync(string invoiceNumber, decimal amount, PaymentMethod method, DateTime date, CancellationToken cancellationToken = default);

	/// <summary>
	/// Invoices with an amount due greater than zero (open, partly paid or overdue), soonest due date first
	/// </summary>
	public Task<PagedResult<Invoice>> GetOpenInvoicesAsync(int? customerId, int page, int pageSize, CancellationToken cancellationToken = default);

	/// <summary>
	/// Invoices with an amount due greater than zero whose due date has passed, soonest due date first
	/// </summary>
	public Task<PagedResult<Invoice>> GetOverdueInvoicesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
