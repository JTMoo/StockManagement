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


	public Task<Supplier> GetSupplierByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		return _db.Suppliers.SingleOrDefaultAsync(supplier => supplier.Id == id, cancellationToken)!;
	}

	public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync(CancellationToken cancellationToken = default)
	{
		return await _db.Suppliers.ToListAsync(cancellationToken);
	}

	/// <exception cref="SupplierNameAlreadyExistsException">Name already in use</exception>
	public async Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
	{
		_db.Suppliers.Add(supplier);
		await this.SaveChangesAsync(cancellationToken);
	}

	/// <exception cref="SupplierNameAlreadyExistsException">Name already in use</exception>
	public async Task<int> UpdateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
	{
		_db.Suppliers.Update(supplier);
		await this.SaveChangesAsync(cancellationToken);
		return 1;
	}

	/// <exception cref="SupplierInUseException">Assigned to at least one stock item</exception>
	public async Task<int> DeleteSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default)
	{
		_db.Suppliers.Remove(supplier);
		await this.SaveChangesAsync(cancellationToken);
		return 1;
	}

	private async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _db.SaveChangesAsync(cancellationToken);
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
