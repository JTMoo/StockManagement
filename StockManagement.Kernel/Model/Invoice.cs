using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Invoice : BaseDocument
{
	private SaleCondition saleCondition;
	private DateTime date;
	private DateTime expirationDate;
	private long total;
	private int number;


	public Invoice()
	{
	}


	#region Properties
	public int Number
	{
		get { return this.number; }
		set { this.SetField(ref this.number, value); }
	}
	public DateTime Date
	{
		get { return this.date; }
		set { this.SetField(ref this.date, value); }
	}
	public DateTime ExpirationDate
	{
		get { return this.expirationDate; }
		set { this.SetField(ref this.expirationDate, value); }
	}
	public long Total
	{
		get { return this.total; }
		set { this.SetField(ref this.total, value); }
	}
	public SaleCondition SaleCondition
	{
		get { return this.saleCondition; }
		set { this.SetField(ref this.saleCondition, value); }
	}
	public Customer Customer { get; set; }
	#endregion Properties
}
