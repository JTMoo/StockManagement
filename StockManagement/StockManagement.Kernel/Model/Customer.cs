using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Model;


public class Customer : NotificationBase
{
	public Customer()
	{
	}


	#region Properties
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }
	#endregion Properties
}
