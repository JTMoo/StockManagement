using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


[Display(ResourceType = typeof(Language.Suppliers), Name = nameof(Language.Suppliers.supplier))]
public class Supplier : BaseDocument
{
	private string _name = string.Empty;
	private string _contactName = string.Empty;
	private string _country = string.Empty;
	private string _currency = string.Empty;
	private int _leadTimeDays;
	private string _miscellaneous = string.Empty;


	public Supplier()
	{
	}

	public Supplier(string name, string contactName = "", string country = "", string currency = "", int leadTimeDays = 0, string misc = "")
	{
		this.Name = name;
		this.ContactName = contactName;
		this.Country = country;
		this.Currency = currency;
		this.LeadTimeDays = leadTimeDays;
		this.Miscellaneous = misc;
	}

	[Display(ResourceType = typeof(Language.Common), Name = nameof(Language.Common.name))]
	public string Name
	{
		get { return _name; }
		set { this.SetField(ref _name, value); }
	}

	[Display(ResourceType = typeof(Language.Suppliers), Name = nameof(Language.Suppliers.contactName))]
	public string ContactName
	{
		get { return _contactName; }
		set { this.SetField(ref _contactName, value); }
	}

	[Display(ResourceType = typeof(Language.Suppliers), Name = nameof(Language.Suppliers.country))]
	public string Country
	{
		get { return _country; }
		set { this.SetField(ref _country, value); }
	}

	[Display(ResourceType = typeof(Language.Suppliers), Name = nameof(Language.Suppliers.currency))]
	public string Currency
	{
		get { return _currency; }
		set { this.SetField(ref _currency, value); }
	}

	[Display(ResourceType = typeof(Language.Suppliers), Name = nameof(Language.Suppliers.leadTimeDays))]
	public int LeadTimeDays
	{
		get { return _leadTimeDays; }
		set { this.SetField(ref _leadTimeDays, value); }
	}

	[Display(ResourceType = typeof(Language.Common), Name = nameof(Language.Common.miscellaneous))]
	public string Miscellaneous
	{
		get { return _miscellaneous; }
		set { this.SetField(ref _miscellaneous, value); }
	}
}
