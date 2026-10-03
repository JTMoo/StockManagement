using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Sales.Core.Contracts;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Sales.Core;


/// <remarks>
/// DNIT comprobante-type codes (1 = Factura, 5 = Nota de Crédito) are the commonly published values; unverified
/// against the current DNIT spec for this export (#123) - same caveat as <see cref="IvaBookRow"/>.
/// </remarks>
internal class IvaBookExportService(IInvoiceServiceProvider invoiceServiceProvider, ICreditNoteServiceProvider creditNoteServiceProvider, ISettingsService settingsService) : IIvaBookExportService
{
	private const string FacturaCode = "1";
	private const string CreditNoteCode = "5";

	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;
	private readonly ICreditNoteServiceProvider _creditNoteServiceProvider = creditNoteServiceProvider;
	private readonly ISettingsService _settingsService = settingsService;


	public async Task<IReadOnlyList<IvaBookRow>> GetRowsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
	{
		var companySettings = await _settingsService.GetCompanySettingsAsync(cancellationToken);
		var digits = companySettings.CurrencyDecimalDigits;

		List<IvaBookRow> rows = [];
		string? cursor = null;
		do
		{
			cancellationToken.ThrowIfCancellationRequested();
			var page = await _invoiceServiceProvider.GetInvoicesAsync(null, from, to, cursor, 100, cancellationToken);
			rows.AddRange(page.Items.Select(invoice => ToRow(invoice, digits)));
			cursor = page.NextCursor;
		} while (cursor != null);

		var creditNotes = await _creditNoteServiceProvider.GetCreditNotesAsync(cancellationToken);
		rows.AddRange(creditNotes
			.Where(creditNote => creditNote.Date >= from && creditNote.Date <= to)
			.Select(creditNote => ToRow(creditNote, digits)));

		return [.. rows.OrderByDescending(row => row.Date)];
	}

	private static IvaBookRow ToRow(Invoice invoice, int digits)
	{
		var breakdown = IvaBreakdown.Calculate(invoice.Items ?? [], digits);
		return new IvaBookRow(FacturaCode, invoice.Number, invoice.Date, invoice.Customer?.IdentificationNumber ?? "", invoice.Customer?.Display ?? "",
			breakdown.Taxed10, breakdown.Vat10, breakdown.Taxed5, breakdown.Vat5, breakdown.Exempt, invoice.Total);
	}

	/// <remarks>A credit note cancels its whole invoice (#56): its IVA breakdown is the cancelled invoice's own.</remarks>
	private static IvaBookRow ToRow(CreditNote creditNote, int digits)
	{
		var breakdown = IvaBreakdown.Calculate(creditNote.Invoice?.Items ?? [], digits);
		return new IvaBookRow(CreditNoteCode, creditNote.Number.ToString(), creditNote.Date, creditNote.Invoice?.Customer?.IdentificationNumber ?? "",
			creditNote.Invoice?.Customer?.Display ?? "", breakdown.Taxed10, breakdown.Vat10, breakdown.Taxed5, breakdown.Vat5, breakdown.Exempt, creditNote.Total);
	}
}
