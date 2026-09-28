using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Sales.Core;


internal class CreditNoteService(ICreditNoteServiceProvider creditNoteServiceProvider) : ICreditNoteService
{
	private readonly ICreditNoteServiceProvider _creditNoteServiceProvider = creditNoteServiceProvider;


	public async Task<int> GetNextCreditNoteNumberAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var creditNotes = await _creditNoteServiceProvider.GetCreditNotesAsync() ?? [];
		return SequenceNumber.Next(creditNotes.Select(creditNote => creditNote.Number), 1);
	}

	public async Task<CreditNoteResult> CancelInvoiceAsync(Invoice invoice, string reason, DateTime date, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(invoice);

		var creditNote = new CreditNote()
		{
			Number = await this.GetNextCreditNoteNumberAsync(cancellationToken),
			Date = date,
			Reason = reason,
			Invoice = invoice,
			Total = invoice.Total,
			Tax = invoice.Tax
		};

		var stored = await _creditNoteServiceProvider.TryAddCreditNoteAsync(creditNote, cancellationToken);
		return stored ? CreditNoteResult.Success with { CreditNote = creditNote } : CreditNoteResult.Failed;
	}
}
