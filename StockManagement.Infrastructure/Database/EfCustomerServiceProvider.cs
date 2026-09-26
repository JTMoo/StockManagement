using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="ICustomerServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfCustomerServiceProvider(AppDbContext db) : ICustomerServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<Customer> GetCustomerAsync(int customerId)
	{
		return _db.Customers.SingleOrDefaultAsync(customer => customer.CustomerId == customerId)!;
	}

	public Task<Customer> GetCustomerByIdAsync(string id)
	{
		return _db.Customers.SingleOrDefaultAsync(customer => customer.Id == id)!;
	}

	public async Task<IEnumerable<Customer>> GetCustomersAsync()
	{
		return await _db.Customers.ToListAsync();
	}

	/// <exception cref="CustomerIdAlreadyExistsException">Customer id already in use</exception>
	public async Task AddCustomerAsync(Customer customer)
	{
		_db.Customers.Add(customer);
		await this.SaveChangesAsync();
	}

	/// <exception cref="CustomerIdAlreadyExistsException">Customer id already in use</exception>
	public async Task AddManyCustomersAsync(IList<Customer> customers)
	{
		if (customers is not { Count: > 0 }) return;

		_db.Customers.AddRange(customers);
		await this.SaveChangesAsync();
	}

	/// <exception cref="CustomerIdAlreadyExistsException">Customer id already in use</exception>
	public async Task<int> UpdateCustomerAsync(Customer customer)
	{
		_db.Customers.Update(customer);
		await this.SaveChangesAsync();
		return 1;
	}

	public async Task<int> DeleteCustomerAsync(Customer customer)
	{
		_db.Customers.Remove(customer);
		await this.SaveChangesAsync();
		return 1;
	}

	private async Task SaveChangesAsync()
	{
		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			throw new CustomerIdAlreadyExistsException();
		}
	}
}
