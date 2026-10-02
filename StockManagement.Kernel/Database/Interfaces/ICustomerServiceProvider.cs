using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ICustomerServiceProvider
{
	public Task<Customer> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
	public Task<Customer> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);
	public Task<IEnumerable<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
	public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default);
	public Task AddManyCustomersAsync(IList<Customer> customers, CancellationToken cancellationToken = default);
}
