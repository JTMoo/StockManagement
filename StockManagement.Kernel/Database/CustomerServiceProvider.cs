using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class CustomerServiceProvider(IDatabase database) : ICustomerServiceProvider
{
	private readonly IDatabase _database = database;

	public Task AddCustomerAsync(Customer customer)
	{
		var collection = _database.ConnectToMongo<Customer>();
		return collection.InsertOneAsync(customer);
	}

	public Task<DeleteResult> DeleteCustomerAsync(Customer customer)
	{
		return _database.Delete<Customer>(customer);
	}

	public Task<Customer> GetCustomerAsync(int customerId)
	{
		return _database.GetOneAsync<Customer>(customer => customer.CustomerId == customerId);
	}

	public Task<IEnumerable<Customer>> GetCustomersAsync()
	{
		return _database.GetAll<Customer>();
	}

	public Task<ReplaceOneResult> UpdateCustomerAsync(Customer customer)
	{
		var collection = _database.ConnectToMongo<Customer>();
		var filter = Builders<Customer>.Filter.Eq("Id", customer.CustomerId);
		// Upsert means: replace if existent - insert if not existent
		return collection.ReplaceOneAsync(filter, customer, new ReplaceOptions { IsUpsert = true });
	}
}
