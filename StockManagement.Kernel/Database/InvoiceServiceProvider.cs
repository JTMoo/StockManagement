using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class InvoiceServiceProvider(IDatabase database) : IInvoiceServiceProvider
{
	private readonly IDatabase _database = database;


	/// <summary>
	/// Tries to add <see cref="Invoice"/> if its unique Property doesn't already exist in the database
	/// </summary>
	/// <param name="invoice"></param>
	/// <returns></returns>
	/// <exception cref="MongoBulkWriteException">Thrown when unique field already exists</exception>
	public Task AddInvoiceAsync(Invoice invoice)
	{
		var collection = _database.ConnectToMongo<Invoice>();
		return collection.InsertOneAsync(invoice);
	}

	public Task<DeleteResult> DeleteInvoiceAsync(Invoice invoice)
	{
		return _database.Delete<Invoice>(invoice);
	}

	public Task<Invoice> GetInvoiceAync(int invoiceNumber)
	{
		return _database.GetOneAsync<Invoice>(invoice => invoice.Number == invoiceNumber);
	}

	public Task<IEnumerable<Invoice>> GetInvoicesAsync()
	{
		return _database.GetAll<Invoice>();
	}

	public Task<ReplaceOneResult> UpdateInvoiceAsync(Invoice invoice)
	{
		var collection = _database.ConnectToMongo<Invoice>();
		var filter = Builders<Invoice>.Filter.Eq("Id", invoice.Number);
		// Upsert means: replace if existent - insert if not existent
		return collection.ReplaceOneAsync(filter, invoice, new ReplaceOptions { IsUpsert = true });
	}
}
