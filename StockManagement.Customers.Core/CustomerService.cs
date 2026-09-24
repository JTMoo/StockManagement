using StockManagement.Customers.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Util;

namespace StockManagement.Customers.Core;


internal class CustomerService(ICustomerServiceProvider customerServiceProvider) : ICustomerService
{
	public const int FirstCustomerId = 1001;

	private readonly ICustomerServiceProvider _customerServiceProvider = customerServiceProvider;


	/// <remarks>Still "highest + 1" over all stored customers; a counter document with $inc is the planned replacement.</remarks>
	public async Task<int> GetNextCustomerIdAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var customers = await _customerServiceProvider.GetCustomersAsync() ?? [];
		return SequenceNumber.Next(customers.Select(customer => customer.CustomerId), FirstCustomerId);
	}
}
