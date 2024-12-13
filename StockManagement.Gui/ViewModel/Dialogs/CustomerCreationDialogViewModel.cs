using StockManagement.Kernel.Model;
using StockManagement.Kernel.Database;
using System.Threading.Tasks;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class CustomerCreationDialogViewModel : DialogViewModelBase
{
	private Customer _customer;
	private readonly ICustomerServiceProvider _customerServiceProvider;

	public CustomerCreationDialogViewModel(ICustomerServiceProvider customerServiceProvider, int customerId) : this(customerServiceProvider)
	{
		_customer = new()
		{
			CustomerId = customerId
		};
	}
	public CustomerCreationDialogViewModel(ICustomerServiceProvider customerServiceProvider, Customer customer) : this(customerServiceProvider)
	{
		_customer = customer;
	}

	private CustomerCreationDialogViewModel(ICustomerServiceProvider customerServiceProvider)
	{
		_customerServiceProvider = customerServiceProvider;
	}

	public Customer Customer
	{
		get { return _customer; }
		set { this.SetField(ref _customer, value); }
	}


	public override async void Confirm(string obj)
	{
		await CreateOrUpdateCustomer();
		base.Confirm(obj);
	}

	private async Task CreateOrUpdateCustomer()
	{
		var customer = await _customerServiceProvider.GetCustomerAsync(this.Customer.CustomerId);
		if (customer == null)
		{
			await _customerServiceProvider.AddCustomerAsync(this.Customer);
		}
		else
		{
			var result = await _customerServiceProvider.UpdateCustomerAsync(this.Customer).ContinueWith(task => task.Result.ModifiedCount == 1);
		}
	}
}
