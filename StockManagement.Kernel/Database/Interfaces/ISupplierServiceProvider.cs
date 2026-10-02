using StockManagement.Kernel.Database;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ISupplierServiceProvider
{
	public Task<Supplier> GetSupplierByIdAsync(string id, CancellationToken cancellationToken = default);
	public Task<IEnumerable<Supplier>> GetAllSuppliersAsync(CancellationToken cancellationToken = default);

	/// <summary>Suppliers by <see cref="Supplier.Name"/> then <see cref="BaseDocument.Id"/>, one page at a time</summary>
	public Task<CursorPage<Supplier>> GetSuppliersAsync(string? cursor, int pageSize, CancellationToken cancellationToken = default);

	/// <exception cref="Exceptions.SupplierNameAlreadyExistsException">Name already in use</exception>
	public Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	/// <exception cref="Exceptions.SupplierNameAlreadyExistsException">Name already in use</exception>
	public Task<int> UpdateSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	/// <exception cref="Exceptions.SupplierInUseException">Assigned to at least one stock item</exception>
	public Task<int> DeleteSupplierAsync(Supplier supplier, CancellationToken cancellationToken = default);
}
