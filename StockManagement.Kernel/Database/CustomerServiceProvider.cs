using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class CustomerServiceProvider(IDatabase database) : ICustomerServiceProvider
{
	private readonly IDatabase _database = database;

	public Task AddCustomerAsync(Customer customer)
	{
		return _database.Add<Customer>(customer);
	}

	public Task<DeleteResult> DeleteCustomerAsync(Customer customer)
	{
		return _database.Delete<Customer>(customer);
	}

	public Task<IEnumerable<Customer>> GetCustomersAsync()
	{
		return _database.GetAll<Customer>();
	}

	public Task<ReplaceOneResult> UpdateCustomerAsync(Customer customer)
	{
		return _database.Update<Customer>(customer);
	}
}
