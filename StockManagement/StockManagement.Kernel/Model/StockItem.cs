using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


public abstract class StockItem : NotificationBase
{
	private string _description = string.Empty;
	private Location _location = new Location();
	private string _name = string.Empty;
	private ManufacturerType _manufacturer;
	private int _price;


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
		set { this.SetField(ref _name, value); }
	}

	public string Description
	{
		get { return _description; }
		set { this.SetField(ref _description, value); }
	}

	public Location Location
	{
		get { return _location; }
		set { this.SetField(ref _location, value); }
	}

	public int Price
	{
		get { return _price; }
		set { this.SetField(ref _price, value); }
	}

	public ManufacturerType Manufacturer
	{
		get { return _manufacturer; }
		set { this.SetField(ref _manufacturer, value); }
	}

	internal abstract void Register();
}