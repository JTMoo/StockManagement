using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public class Location(IsleType isle = IsleType.None, ShelfType shelf = ShelfType.None) : NotificationBase
{
	private IsleType _isle = isle;
	private ShelfType _shelf = shelf;

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public IsleType Isle
	{
		get { return _isle; }
		internal set { this.SetField(ref _isle, value); }
	}

	public ShelfType Shelf
	{
		get { return _shelf; }
		internal set { this.SetField(ref _shelf, value); }
	}
}