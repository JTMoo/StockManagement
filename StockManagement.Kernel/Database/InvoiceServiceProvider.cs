using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class InvoiceServiceProvider : IInvoiceServiceProvider
{
	public Task AddInvoiceAsync(Invoice customer)
	{
		throw new NotImplementedException();
	}

	public Task<DeleteResult> DeleteInvoiceAsync(Invoice customer)
	{
		throw new NotImplementedException();
	}

	public Task<Invoice> GetInvoiceAync(int invoiceId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Invoice>> GetInvoicesAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ReplaceOneResult> UpdateInvoiceAsync(Invoice customer)
	{
		throw new NotImplementedException();
	}
}
