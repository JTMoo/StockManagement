using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Model;

public class Transaction : NotificationBase
{
	public Transaction(string stockItemName, DateTime time, Kind kind, int amount)
	{
		this.StockItemName = stockItemName;
		this.Time = time;
		this.SelectedKind = kind;
		this.Amount = amount;
	}

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public string StockItemName { get; private set; }

	public DateTime Time { get; private set; }

	public Kind SelectedKind { get; private set; }

	public int Amount { get; private set; }

	public enum Kind
	{
		Price = 0,

		Amount
	}
}
