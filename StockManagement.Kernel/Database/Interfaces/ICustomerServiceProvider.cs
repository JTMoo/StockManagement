using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ICustomerServiceProvider
{
	public Task<Customer> GetCustomerAsync(int customerId);
	public Task<IEnumerable<Customer>> GetCustomersAsync();

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateCustomerAsync(Customer customer);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteCustomerAsync(Customer customer);
	public Task AddCustomerAsync(Customer customer);
}
