using StockManagement.Kernel.Model;

namespace StockManagement.Customers.Core.Contracts;


public interface ICustomerService
{
	/// <summary>
	/// Id for the next new customer
	/// </summary>
	public Task<int> GetNextCustomerIdAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores <paramref name="customer"/> under the next customer id
	/// </summary>
	/// <remarks>Overwrites <see cref="Customer.CustomerId"/>.</remarks>
	public Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
}
