using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.invoice))]
public class Invoice : BaseDocument
{
	private SaleCondition saleCondition;
	private DateTime date;
	private DateTime expirationDate;
	private long total;
	private long tax;
	private int number;


	public Invoice()
	{
	}


	#region Properties
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.invoiceId))]
	public int Number
	{
		get { return this.number; }
		set { this.SetField(ref this.number, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.tax))]
	public long Tax
	{
		get { return this.tax; }
		set { this.SetField(ref this.tax, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.creationDate))]
	public DateTime Date
	{
		get { return this.date; }
		set { this.SetField(ref this.date, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.expirationDate))]
	public DateTime ExpirationDate
	{
		get { return this.expirationDate; }
		set { this.SetField(ref this.expirationDate, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.total))]
	public long Total
	{
		get { return this.total; }
		set { this.SetField(ref this.total, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.saleCondition))]
	public SaleCondition SaleCondition
	{
		get { return this.saleCondition; }
		set { this.SetField(ref this.saleCondition, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.customer))]
	public Customer Customer { get; set; }
	public List<ShoppingCartItem> Items { get; set; }
	#endregion Properties
}
