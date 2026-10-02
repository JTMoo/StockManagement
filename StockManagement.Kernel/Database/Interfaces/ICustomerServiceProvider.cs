using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ICustomerServiceProvider
{
	public Task<Customer> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
	public Task<Customer> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);
	public Task<IEnumerable<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default);

	/// <summary>Customers by <see cref="Customer.Name"/> then <see cref="BaseDocument.Id"/>, one page at a time</summary>
	public Task<CursorPage<Customer>> GetCustomersAsync(string? cursor, int pageSize, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
	public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
	public Task AddManyCustomersAsync(IList<Customer> customers, CancellationToken cancellationToken = default);
}
