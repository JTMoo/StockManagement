using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="ISupplierServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfSupplierServiceProvider(AppDbContext db) : ISupplierServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<Supplier> GetSupplierByIdAsync(string id)
	{
		return _db.Suppliers.SingleOrDefaultAsync(supplier => supplier.Id == id)!;
	}

	public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
	{
		return await _db.Suppliers.ToListAsync();
	}

	/// <exception cref="SupplierNameAlreadyExistsException">Name already in use</exception>
	public async Task AddSupplierAsync(Supplier supplier)
	{
		_db.Suppliers.Add(supplier);
		await this.SaveChangesAsync();
	}

	/// <exception cref="SupplierNameAlreadyExistsException">Name already in use</exception>
	public async Task<int> UpdateSupplierAsync(Supplier supplier)
	{
		_db.Suppliers.Update(supplier);
		await this.SaveChangesAsync();
		return 1;
	}

	/// <exception cref="SupplierInUseException">Assigned to at least one stock item</exception>
	public async Task<int> DeleteSupplierAsync(Supplier supplier)
	{
		_db.Suppliers.Remove(supplier);
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
			throw new SupplierNameAlreadyExistsException();
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23503" })
		{
			throw new SupplierInUseException();
		}
	}
}
