using MongoDB.Driver;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


public class InvoiceServiceProvider(IDatabase database) : IInvoiceServiceProvider
{
	private readonly IDatabase _database = database;


	public Task AddInvoiceAsync(Invoice invoice)
	{
		// This is not ideal - the add process should fail if the number is taken!
		var invoiceExistsCheck = this.GetInvoiceAync(invoice.Number).ContinueWith(task => task.Result != null).Result;
		if (invoiceExistsCheck) throw new InvoiceNumberAlreadyExistsException();

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
