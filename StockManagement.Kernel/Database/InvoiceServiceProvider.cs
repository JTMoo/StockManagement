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

	public async Task<IReadOnlyList<string>> TryAddSaleAsync(Invoice invoice, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(invoice);

		var items = invoice.Items ?? [];
		var stockItems = _database.ConnectToMongo<StockItem>();
		var transactions = _database.ConnectToMongo<Transaction>();
		var options = new FindOneAndUpdateOptions<StockItem> { ReturnDocument = ReturnDocument.After };
		List<(ShoppingCartItem Item, int AmountLeft)> taken = [];

		using var session = await _database.StartSessionAsync(cancellationToken);
		session.StartTransaction();

		foreach (var item in items)
		{
			var filter = Builders<StockItem>.Filter.Where(stockItem => stockItem.Code == item.StockItem.Code && stockItem.Amount >= item.Amount);
			var update = Builders<StockItem>.Update.Inc(stockItem => stockItem.Amount, -item.Amount);
			if (await stockItems.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken) is not StockItem stored)
			{
				await session.AbortTransactionAsync(cancellationToken);
				return [item.StockItem.Name];
			}

			await transactions.InsertOneAsync(session, new Transaction(stored, DateTime.Now, Transaction.Kind.Amount, -item.Amount), cancellationToken: cancellationToken);
			taken.Add((item, stored.Amount));
		}

		await _database.ConnectToMongo<Invoice>().InsertOneAsync(session, invoice, cancellationToken: cancellationToken);
		await session.CommitTransactionAsync(cancellationToken);

		taken.ForEach(line => line.Item.StockItem.Amount = line.AmountLeft);
		return [];
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
		var filter = Builders<Invoice>.Filter.Eq("Id", invoice.Id);
		// Upsert means: replace if existent - insert if not existent
		return collection.ReplaceOneAsync(filter, invoice, new ReplaceOptions { IsUpsert = true });
	}
}
