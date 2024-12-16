using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public interface IInvoiceServiceProvider
{
	public Task<Invoice> GetInvoiceAync(int invoiceId);
	public Task<IEnumerable<Invoice>> GetInvoicesAsync();
	public Task<ReplaceOneResult> UpdateInvoiceAsync(Invoice customer);
	public Task<DeleteResult> DeleteInvoiceAsync(Invoice customer);
	public Task AddInvoiceAsync(Invoice customer);
}
