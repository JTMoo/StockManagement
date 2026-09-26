using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ICustomerServiceProvider
{
	public Task<Customer> GetCustomerAsync(int customerId);
	public Task<Customer> GetCustomerByIdAsync(string id);
	public Task<IEnumerable<Customer>> GetCustomersAsync();

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateCustomerAsync(Customer customer);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteCustomerAsync(Customer customer);
	public Task AddCustomerAsync(Customer customer);
	public Task AddManyCustomersAsync(IList<Customer> customers);
}
