using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Database;


public abstract class BaseDocument : NotificationBase
{

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; set; }
}
