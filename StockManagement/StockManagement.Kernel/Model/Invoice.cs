using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace StockManagement.Kernel.Model;


public class Invoice : NotificationBase
{
	public Invoice()
	{
	}


	#region Properties
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public Customer Customer { get; set; }
	#endregion Properties
}
