using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Payment : NotificationBase
{
	private DateTime date;
	private decimal amount;
	private PaymentMethod method;


	public DateTime Date
	{
		get { return this.date; }
		set { this.SetField(ref this.date, value); }
	}
	public decimal Amount
	{
		get { return this.amount; }
		set { this.SetField(ref this.amount, value); }
	}
	public PaymentMethod Method
	{
		get { return this.method; }
		set { this.SetField(ref this.method, value); }
	}
}
