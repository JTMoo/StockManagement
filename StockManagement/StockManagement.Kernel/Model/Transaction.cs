using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Model;

public class Transaction(StockItem stockItem, DateTime time, Transaction.Kind kind, int amount) : NotificationBase
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public StockItem StockItem { get; private set; } = stockItem;

	public Invoice Invoice { get; internal set; }

	public DateTime Time { get; private set; } = time;

	public Kind SelectedKind { get; private set; } = kind;

	public int Amount { get; private set; } = amount;

	public enum Kind
	{
		Price = 0,

		Amount
	}
}
