using System.ComponentModel.DataAnnotations;
using StockManagement.Kernel.Database;

namespace StockManagement.Kernel.Model;


public class Customer : BaseDocument
{
	private int customerId = -1;
	private string name = string.Empty;
	private string lastname = string.Empty;
	private string address = string.Empty;
	private string phoneNumber = string.Empty;
	private string identificationNumber = string.Empty;
	private string postboxNumber = string.Empty;
	private string email = string.Empty;
	private string miscellaneous = string.Empty;


	public Customer()
	{
	}


	#region Properties
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.customerId))]
	public int CustomerId
	{
		get { return this.customerId; }
		set { this.SetField(ref this.customerId, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.name))]
	public string Name
	{
		get { return this.name; }
		set { this.SetField(ref this.name, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.lastname))]
	public string Lastname
	{
		get { return this.lastname; }
		set { this.SetField(ref this.lastname, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.address))]
	public string Address
	{
		get { return this.address; }
		set { this.SetField(ref this.address, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.phoneNumber))]
	public string PhoneNumber
	{
		get { return this.phoneNumber; }
		set { this.SetField(ref this.phoneNumber, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.identificationNumber))]
	public string IdentificationNumber
	{
		get { return this.identificationNumber; }
		set { this.SetField(ref this.identificationNumber, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.postboxNumber))]
	public string PostboxNumber
	{
		get { return this.postboxNumber; }
		set { this.SetField(ref this.postboxNumber, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.email))]
	public string Email
	{
		get { return this.email; }
		set { this.SetField(ref this.email, value); }
	}
	[Display(ResourceType = typeof(Language.Resources), Name = nameof(Language.Resources.miscellaneous))]
	public string Miscellaneous
	{
		get { return this.miscellaneous; }
		set { this.SetField(ref this.miscellaneous, value); }
	}
	public string Display
	{
		get { return string.Join(" ", this.Name, this.Lastname, this.CustomerId); }
	}
	#endregion Properties
}
