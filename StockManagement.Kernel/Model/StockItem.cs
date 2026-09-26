using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.stockItem))]
public class StockItem : BaseDocument
{
	private string _code = string.Empty;
	private string _description = string.Empty;
	private string _location = string.Empty;
	private string _name = string.Empty;
	private string _miscellaneous = string.Empty;
	private string _manufacturer = string.Empty;
	private double _price;
	private double _factor;
	private int _amount;

	public StockItem ()
	{
	}

	public StockItem (string name, string code = "", string description = "", int amount=1, int price = 0, string manufacturer = "", string misc = "") 
	{
		this.Name = name;
		this.Code = code;
		this.Description = description;
		this.Amount = amount;
		this.Price = price;
		this.Manufacturer = manufacturer;
		this.Miscellaneous = misc;
	}

	[Display(ResourceType = typeof(Language.Common), Name = nameof(Language.Common.name))]
	public string Name
	{
		get { return _name; }
		set { this.SetField(ref _name, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.code))]
	public string Code
	{
		get { return _code; }
		set { this.SetField(ref _code, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.amount))]
	public int Amount
	{
		get { return _amount; }
		set { this.SetField(ref _amount, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.description))]
	public string Description
	{
		get { return _description; }
		set { this.SetField(ref _description, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.location))]
	public string Location
	{
		get { return _location; }
		set { this.SetField(ref _location, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.price))]
	public double Price
	{
		get { return _price; }
		set { this.SetField(ref _price, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.factor))]
	public double Factor
	{
		get { return _factor; }
		set { this.SetField(ref _factor, value); }
	}

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.manufacturer))]
	[BsonSerializer(typeof(ManufacturerSerializer))]
	public string Manufacturer
	{
		get { return _manufacturer; }
		set { this.SetField(ref _manufacturer, value?.Trim() ?? string.Empty); }
	}

	[Display(ResourceType = typeof(Language.Common), Name = nameof(Language.Common.miscellaneous))]
	public string Miscellaneous
	{
		get { return _miscellaneous; }
		set { this.SetField(ref _miscellaneous, value); }
	}
}