namespace StockManagement.Kernel.Model;


public class ShoppingCartItem(StockItem item) : NotificationBase
{
	private int amount = 1;
	private int discount = 0;


	public StockItem StockItem { get; set; } = item;

	public int Discount
	{
		get { return this.discount; }
		set { this.SetField(ref this.discount, value); }
	}

	public int Amount
	{
		get { return this.amount; }
		set 
		{ 
			if (this.StockItem != null && value > this.StockItem.Amount)
			{
				value = this.StockItem.Amount;
			}
			this.SetField(ref this.amount, value); 
		}
	}
}
