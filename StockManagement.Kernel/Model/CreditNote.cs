using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


/// <summary>
/// Cancels an <see cref="Invoice"/>: stock is returned, the invoice itself is kept and marked <see cref="Invoice.IsCancelled"/> (#56)
/// </summary>
[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.creditNote))]
public class CreditNote : BaseDocument
{
	private int number;
	private DateTime date;
	private string reason = string.Empty;
	private decimal total;
	private decimal tax;


	public CreditNote()
	{
	}


	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.creditNoteId))]
	public int Number
	{
		get { return this.number; }
		set { this.SetField(ref this.number, value); }
	}
	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.creationDate))]
	public DateTime Date
	{
		get { return this.date; }
		set { this.SetField(ref this.date, value); }
	}
	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.cancelReason))]
	public string Reason
	{
		get { return this.reason; }
		set { this.SetField(ref this.reason, value); }
	}
	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.total))]
	public decimal Total
	{
		get { return this.total; }
		set { this.SetField(ref this.total, value); }
	}
	[Display(ResourceType = typeof(Language.Invoices), Name = nameof(Language.Invoices.tax))]
	public decimal Tax
	{
		get { return this.tax; }
		set { this.SetField(ref this.tax, value); }
	}
	public Invoice Invoice { get; set; }
}
