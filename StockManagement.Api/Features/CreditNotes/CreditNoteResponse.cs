using StockManagement.Kernel.Model;

namespace StockManagement.Api.Features.CreditNotes;


/// <summary>
/// Stored credit note; cancels the referenced invoice in full
/// </summary>
public sealed record CreditNoteResponse(int Number, DateTime Date, string Reason, decimal Total, decimal Tax, int InvoiceNumber)
{
	public static CreditNoteResponse From(CreditNote creditNote)
	{
		return new(creditNote.Number, creditNote.Date, creditNote.Reason, creditNote.Total, creditNote.Tax, creditNote.Invoice.Number);
	}
}
