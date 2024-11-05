namespace StockManagement.Kernel.Model;


public class ShoppingCartItem(StockItem item) : NotificationBase
{
	private int amount = 1;


	public int Amount
	{
		get { return this.amount; }
		set 
		{ 
			if (value > this.StockItem.Amount)
			{
				value = this.StockItem.Amount;
			}
			this.SetField(ref this.amount, value); 
		}
	}

	public StockItem StockItem { get; set; } = item;
}
