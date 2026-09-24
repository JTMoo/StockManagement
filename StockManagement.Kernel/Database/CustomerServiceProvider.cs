using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class CustomerServiceProvider(IDatabase database) : ICustomerServiceProvider
{
	private readonly IDatabase _database = database;


	/// <summary>
	/// Tries to add <see cref="Customer"/> if its unique Property doesn't already exist in the database
	/// </summary>
	/// <param name="customer"></param>
	/// <returns></returns>
	/// <exception cref="MongoBulkWriteException">Thrown if unique Property already exists</exception>
	public Task AddCustomerAsync(Customer customer)
	{
		var collection = _database.ConnectToMongo<Customer>();
		return collection.InsertOneAsync(customer);
	}

	public async Task<int> DeleteCustomerAsync(Customer customer)
	{
		var result = await _database.Delete<Customer>(customer);
		return (int)result.DeletedCount;
	}

	public Task<Customer> GetCustomerAsync(int customerId)
	{
		return _database.GetOneAsync<Customer>(customer => customer.CustomerId == customerId);
	}

	public Task<IEnumerable<Customer>> GetCustomersAsync()
	{
		return _database.GetAll<Customer>();
	}

	public async Task<int> UpdateCustomerAsync(Customer customer)
	{
		var collection = _database.ConnectToMongo<Customer>();
		var filter = Builders<Customer>.Filter.Eq("Id", customer.Id);
		// Upsert means: replace if existent - insert if not existent
		var result = await collection.ReplaceOneAsync(filter, customer, new ReplaceOptions { IsUpsert = true });
		return (int)result.ModifiedCount;
	}
}
