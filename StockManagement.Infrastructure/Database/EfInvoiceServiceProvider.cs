using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="IInvoiceServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfInvoiceServiceProvider(AppDbContext db) : IInvoiceServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<Invoice> GetInvoiceAync(int invoiceNumber)
	{
		return _db.Invoices.SingleOrDefaultAsync(invoice => invoice.Number == invoiceNumber)!;
	}

	public async Task<IEnumerable<Invoice>> GetInvoicesAsync()
	{
		return await _db.Invoices.ToListAsync();
	}

	/// <exception cref="InvoiceNumberAlreadyExistsException">Number already in use</exception>
	public async Task AddInvoiceAsync(Invoice invoice)
	{
		_db.Invoices.Add(invoice);
		await this.SaveChangesAsync();
	}

	/// <exception cref="InvoiceNumberAlreadyExistsException">Number already in use</exception>
	public async Task<int> UpdateInvoiceAsync(Invoice invoice)
	{
		_db.Invoices.Update(invoice);
		await this.SaveChangesAsync();
		return 1;
	}

	public async Task<int> DeleteInvoiceAsync(Invoice invoice)
	{
		_db.Invoices.Remove(invoice);
		await this.SaveChangesAsync();
		return 1;
	}

	/// <remarks>
	/// Stock is decremented with a conditional <c>UPDATE ... WHERE Amount &gt;= @amount</c> per line (no oversell under
	/// concurrent sales, the same guarantee the Mongo conditional <c>$inc</c> gave), then the invoice is stored;
	/// all in one Postgres transaction.
	/// </remarks>
	/// <exception cref="InvoiceNumberAlreadyExistsException">Number already exists; nothing written</exception>
	public async Task<IReadOnlyList<string>> TryAddSaleAsync(Invoice invoice, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(invoice);
		var items = invoice.Items ?? [];

		await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

		List<(ShoppingCartItem Item, int AmountLeft)> taken = [];
		foreach (var item in items)
		{
			var affected = await _db.StockItems
				.Where(stockItem => stockItem.Code == item.StockItem.Code && stockItem.Amount >= item.Amount)
				.ExecuteUpdateAsync(setters => setters.SetProperty(stockItem => stockItem.Amount, stockItem => stockItem.Amount - item.Amount), cancellationToken);

			if (affected == 0)
			{
				await transaction.RollbackAsync(cancellationToken);
				return [item.StockItem.Name];
			}

			// Reuse the tracked instance: a detached one with the same Id would conflict when Invoices.Add() below reaches it via the Items navigation
			var stored = await _db.StockItems.SingleAsync(stockItem => stockItem.Code == item.StockItem.Code, cancellationToken);
			_db.Transactions.Add(new Transaction(stored, DateTime.Now, Transaction.Kind.Amount, -item.Amount));
			item.StockItem = stored;
			taken.Add((item, stored.Amount));
		}

		// Reuse the tracked instance, same reason as the StockItem swap above
		invoice.Customer = await _db.Customers.FindAsync([invoice.Customer.Id], cancellationToken) ?? invoice.Customer;
		_db.Invoices.Add(invoice);

		try
		{
			await _db.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			await transaction.RollbackAsync(cancellationToken);
			throw new InvoiceNumberAlreadyExistsException();
		}

		await transaction.CommitAsync(cancellationToken);

		taken.ForEach(line => line.Item.StockItem.Amount = line.AmountLeft);
		return [];
	}

	private async Task SaveChangesAsync()
	{
		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			throw new InvoiceNumberAlreadyExistsException();
		}
	}
}
