using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public interface ICreditNoteService
{
	/// <summary>
	/// Number for the next new credit note
	/// </summary>
	/// <remarks>Highest stored number + 1, or 1 when no credit note exists.</remarks>
	public Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Cancels <paramref name="invoice"/>: restocks every line and stores a numbered credit note referencing it
	/// </summary>
	/// <remarks>Invoices are never deleted (#56); the invoice itself is kept and marked cancelled.</remarks>
	/// <returns>A failed result when the invoice was already cancelled; nothing is written then</returns>
	public Task<CreditNoteResult> CancelInvoiceAsync(Invoice invoice, string reason, DateTime date, CancellationToken cancellationToken = default);
}
