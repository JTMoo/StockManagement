using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public interface ICustomerServiceProvider
{
	public Task<Customer> GetCustomerAsync(int customerId);
	public Task<IEnumerable<Customer>> GetCustomersAsync();
	public Task<ReplaceOneResult> UpdateCustomerAsync(Customer customer);
	public Task<DeleteResult> DeleteCustomerAsync(Customer customer);
	public Task AddCustomerAsync(Customer customer);
}
