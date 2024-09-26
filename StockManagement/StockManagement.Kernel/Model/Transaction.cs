using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Model;

public class Transaction(string stockItemName, DateTime time, Transaction.Kind kind, int amount) : NotificationBase
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public string StockItemName { get; private set; } = stockItemName;

	public DateTime Time { get; private set; } = time;

	public Kind SelectedKind { get; private set; } = kind;

	public int Amount { get; private set; } = amount;

	public enum Kind
	{
		Price = 0,

		Amount
	}
}
