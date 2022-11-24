using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StockManagement.Kernel.Model;


internal abstract class StockItem : NotificationBase
{
	private int _amount = 0;
	private string _name = string.Empty;
	private string _description = string.Empty;
	private readonly double _price = 0.0;


	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

	public int Amount
	{
		get { return _amount; }
		set { this.SetField(ref this._amount, value); }
	}

	public string Name
	{
		get { return _name; }
		set { this.SetField(ref this._name, value); }
	}

	public string Description
	{
		get { return _description; }
		set { this.SetField(ref this._description, value); }
	}

	public double Price 
	{ 
		get { return this._price; } 
	}
}
