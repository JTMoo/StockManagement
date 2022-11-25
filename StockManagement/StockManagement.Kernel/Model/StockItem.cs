using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


internal abstract class StockItem : NotificationBase
{
	private int _amount = 0;
	private string _name = string.Empty;
	private string _description = string.Empty;


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

	public int Price { get; }

	public ManufacturerType Manufacturer { get; }
}
