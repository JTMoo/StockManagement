using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// Outcome of <see cref="ICreditNoteService.CancelInvoiceAsync"/>.
/// </summary>
/// <param name="AlreadyCancelled">True when the invoice already had a credit note; nothing was written then</param>
public sealed record CreditNoteResult(bool AlreadyCancelled)
{
	/// <summary>
	/// Result of a cancellation that went through
	/// </summary>
	public static CreditNoteResult Success { get; } = new(false);

	/// <summary>
	/// Result of an invoice that was already cancelled
	/// </summary>
	public static CreditNoteResult Failed { get; } = new(true);

	public bool Succeeded => !this.AlreadyCancelled;

	/// <summary>
	/// Stored credit note, set by <see cref="ICreditNoteService.CancelInvoiceAsync"/> on success
	/// </summary>
	public CreditNote? CreditNote { get; init; }
}
