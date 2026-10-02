using System.ComponentModel.DataAnnotations;

namespace StockManagement.Import.Core;


/// <summary>
/// One open-invoice import row: a legacy receivable against an existing <see cref="Kernel.Model.Customer"/>, matched by <see cref="CustomerIdentificationNumber"/>
/// </summary>
internal sealed class OpenInvoiceRow
{
	[Display(ResourceType = typeof(Language.Customers), Name = nameof(Language.Customers.identificationNumber))]
	public string CustomerIdentificationNumber { get; set; } = string.Empty;

	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.invoiceId))]
	public int Number { get; set; }

	[Display(ResourceType = typeof(Language.Import), Name = nameof(Language.Import.invoiceDate))]
	public DateTime Date { get; set; }

	[Display(ResourceType = typeof(Language.Import), Name = nameof(Language.Import.expirationDate))]
	public DateTime ExpirationDate { get; set; }

	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.total))]
	public decimal Total { get; set; }

	[Display(ResourceType = typeof(Language.Import), Name = nameof(Language.Import.tax))]
	public decimal Tax { get; set; }

	[Display(ResourceType = typeof(Language.Import), Name = nameof(Language.Import.amountPaid))]
	public decimal AmountPaid { get; set; }
}
