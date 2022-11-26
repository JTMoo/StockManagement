using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


internal abstract class StockItem : NotificationBase
{
	private string _name = string.Empty;
	private string _description = string.Empty;


	public StockItem ()
	{
	}

	public StockItem (string name, string description = "", int price = 0, ManufacturerType manufacturer = ManufacturerType.None)
	{
		this.Name = name;
		this.Description = description;
		this.Price = price;
		this.Manufacturer = manufacturer;
	}

	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string Id { get; internal set; }

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
