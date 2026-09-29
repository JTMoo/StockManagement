using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ICreditNoteServiceProvider
{
	public Task<CreditNote> GetCreditNoteAsync(int number);
	public Task<IEnumerable<CreditNote>> GetCreditNotesAsync();

	/// <summary>
	/// Restocks every line of <see cref="CreditNote.Invoice"/>, marks it cancelled and stores the credit note, all in one transaction
	/// </summary>
	/// <remarks>Records one <see cref="Transaction"/> per line, with <see cref="CreditNote.Reason"/> as the reason (#8)</remarks>
	/// <returns>False when the invoice is already cancelled; nothing written then</returns>
	/// <exception cref="Exceptions.CreditNoteNumberAlreadyExistsException">Number already exists; nothing written</exception>
	public Task<bool> TryAddCreditNoteAsync(CreditNote creditNote, CancellationToken cancellationToken = default);
}
