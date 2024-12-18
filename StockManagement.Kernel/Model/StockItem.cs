using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.stockItem))]
public class StockItem : BaseDocument
{
	private string _code = string.Empty;
	private string _description = string.Empty;
	private string _location = string.Empty;
	private string _name = string.Empty;
	private string _miscellaneous = string.Empty;
	private ManufacturerType _manufacturer;
	private double _price;
	private double _factor;
	private int _amount;

	public StockItem ()
	{
	}

	public StockItem (string name, string code = "", string description = "", int amount=1, int price = 0, ManufacturerType manufacturer = ManufacturerType.None, string misc = "") 
	{
		this.Name = name;
		this.Code = code;
		this.Description = description;
		this.Amount = amount;
		this.Price = price;
		this.Manufacturer = manufacturer;
		this.Miscellaneous = misc;
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.name))]
	public string Name
	{
		get { return _name; }
		set { this.SetField(ref _name, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.code))]
	public string Code
	{
		get { return _code; }
		set { this.SetField(ref _code, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.amount))]
	public int Amount
	{
		get { return _amount; }
		set { this.SetField(ref _amount, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.description))]
	public string Description
	{
		get { return _description; }
		set { this.SetField(ref _description, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.location))]
	public string Location
	{
		get { return _location; }
		set { this.SetField(ref _location, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.price))]
	public double Price
	{
		get { return _price; }
		set { this.SetField(ref _price, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.factor))]
	public double Factor
	{
		get { return _factor; }
		set { this.SetField(ref _factor, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.manufacturer))]
	public ManufacturerType Manufacturer
	{
		get { return _manufacturer; }
		set { this.SetField(ref _manufacturer, value); }
	}

	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.miscellaneous))]
	public string Miscellaneous
	{
		get { return _miscellaneous; }
		set { this.SetField(ref _miscellaneous, value); }
	}


	public override List<CreateIndexModel<StockItem>> GetIndexCreationModels<StockItem>()
	{
		var creationModels = new List<CreateIndexModel<StockItem>>();

		var options = new CreateIndexOptions() { Unique = true };
		List<StringFieldDefinition<StockItem>> uniqueProperties =
		[
			new StringFieldDefinition<StockItem>(nameof(this.Code))
		];

		uniqueProperties.ForEach(property =>
		{
			var indexDefinition = new IndexKeysDefinitionBuilder<StockItem>().Ascending(property);
			var creationModel = new CreateIndexModel<StockItem>(indexDefinition, options);
			creationModels.Add(creationModel);
		});

		return creationModels;
	}
}