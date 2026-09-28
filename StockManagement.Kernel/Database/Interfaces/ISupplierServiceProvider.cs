using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ISupplierServiceProvider
{
	public Task<Supplier> GetSupplierByIdAsync(string id);
	public Task<IEnumerable<Supplier>> GetAllSuppliersAsync();

	/// <exception cref="Exceptions.SupplierNameAlreadyExistsException">Name already in use</exception>
	public Task AddSupplierAsync(Supplier supplier);

	/// <returns>Rows affected; 1 on success</returns>
	/// <exception cref="Exceptions.SupplierNameAlreadyExistsException">Name already in use</exception>
	public Task<int> UpdateSupplierAsync(Supplier supplier);

	/// <returns>Rows affected; 1 on success</returns>
	/// <exception cref="Exceptions.SupplierInUseException">Assigned to at least one stock item</exception>
	public Task<int> DeleteSupplierAsync(Supplier supplier);
}
