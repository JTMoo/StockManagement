using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public interface IInvoiceServiceProvider
{
	public Task<Invoice> GetInvoiceAync(int invoiceNumber);
	public Task<IEnumerable<Invoice>> GetInvoicesAsync();
	public Task<ReplaceOneResult> UpdateInvoiceAsync(Invoice invoice);
	public Task<DeleteResult> DeleteInvoiceAsync(Invoice invoice);
	public Task AddInvoiceAsync(Invoice invoice);
}
