using StockManagement.Customers.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Customers.Core;


internal class CustomerService(ICustomerServiceProvider customerServiceProvider, ISettingsService settingsService) : ICustomerService
{
	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;
	private readonly ISettingsService _settingsService = settingsService;


	/// <remarks>Still "highest + 1" over all stored customers; a counter document with $inc is the planned replacement.</remarks>
	public async Task<int> GetNextCustomerIdAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var customers = await _customerServiceProvider.GetCustomersAsync() ?? [];
		var companySettings = await _settingsService.GetCompanySettingsAsync(cancellationToken);
		return SequenceNumber.Next(customers.Select(customer => customer.CustomerId), companySettings.FirstCustomerId);
	}

	public async Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(customer);

		customer.CustomerId = await this.GetNextCustomerIdAsync(cancellationToken);
		await _customerServiceProvider.AddCustomerAsync(customer);
		return customer;
	}
}
