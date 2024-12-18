using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace StockManagement.Kernel.Database;


public abstract class BaseDocument : NotificationBase
{

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	internal string Id { get; set; }


	public virtual List<CreateIndexModel<T>> GetIndexCreationModels<T>()
	{
		return [];
	}
}
