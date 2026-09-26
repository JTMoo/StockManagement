using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


public class Transaction(StockItem stockItem, DateTime time, Transaction.Kind kind, int amount, string reason = "") : BaseDocument
{
	/// <summary>
	/// For EF Core materialization
	/// </summary>
	private Transaction() : this(null!, default, default, default)
	{
	}

	public StockItem StockItem { get; private set; } = stockItem;

	public Invoice Invoice { get; internal set; }

	public DateTime Time { get; private set; } = time;

	public Kind SelectedKind { get; private set; } = kind;

	public int Amount { get; private set; } = amount;

	/// <summary>
	/// Why the amount changed; only set by explicit check-in/check-out, empty otherwise
	/// </summary>
	public string Reason { get; private set; } = reason;

	public enum Kind
	{
		Price = 0,

		Amount,

		Deletion
	}
}
