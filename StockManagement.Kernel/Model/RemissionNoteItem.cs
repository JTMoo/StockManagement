namespace StockManagement.Kernel.Model;


/// <summary>
/// One stock item moved by a <see cref="RemissionNote"/>. No price or VAT - a remission note documents a
/// physical movement of goods, not a sale (#162).
/// </summary>
public class RemissionNoteItem(StockItem item) : NotificationBase
{
	private int amount = 1;


	/// <summary>
	/// For EF Core materialization
	/// </summary>
	private RemissionNoteItem() : this(null!)
	{
	}


	public StockItem StockItem { get; set; } = item;

	public int Amount
	{
		get { return this.amount; }
		set { this.SetField(ref this.amount, value); }
	}
}
