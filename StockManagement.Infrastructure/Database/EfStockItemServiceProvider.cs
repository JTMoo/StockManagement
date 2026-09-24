using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="IStockItemServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
/// <remarks>Every write records a <see cref="Transaction"/> in the same <c>SaveChangesAsync</c> call</remarks>
public class EfStockItemServiceProvider(AppDbContext db) : IStockItemServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<StockItem> GetStockItemAsync(string code)
	{
		return _db.StockItems.SingleOrDefaultAsync(item => item.Code == code)!;
	}

	public async Task<IEnumerable<StockItem>> GetAllStockItemsAsync()
	{
		return await _db.StockItems.ToListAsync();
	}

	/// <exception cref="StockItemCodeAlreadyExistsException">Code already in use</exception>
	public async Task AddStockItemAsync(StockItem stockItem)
	{
		_db.StockItems.Add(stockItem);
		_db.Transactions.Add(new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount));
		await this.SaveChangesAsync();
	}

	/// <exception cref="StockItemCodeAlreadyExistsException">Code already in use</exception>
	public async Task AddManyStockItemsAsync(IList<StockItem> stockItem)
	{
		_db.StockItems.AddRange(stockItem);
		await this.SaveChangesAsync();
	}

	/// <exception cref="StockItemCodeAlreadyExistsException">Code already in use</exception>
	public async Task<int> UpdateStockItemAsync(StockItem stockItem)
	{
		// Track StockItem's own state first: Add() on the Transaction below would otherwise graph-fixup StockItem as Added too
		var stored = await _db.StockItems.AsNoTracking().SingleOrDefaultAsync(item => item.Id == stockItem.Id);
		_db.StockItems.Update(stockItem);

		if (stored is not null && stored.Amount != stockItem.Amount)
		{
			_db.Transactions.Add(new Transaction(stockItem, DateTime.Now, Transaction.Kind.Amount, stockItem.Amount - stored.Amount));
		}

		await this.SaveChangesAsync();
		return 1;
	}

	public async Task<int> DeleteStockItemAsync(StockItem stockItem)
	{
		// Same ordering reason as UpdateStockItemAsync
		_db.StockItems.Remove(stockItem);
		_db.Transactions.Add(new Transaction(stockItem, DateTime.Now, Transaction.Kind.Deletion, 1));
		await this.SaveChangesAsync();
		return 1;
	}

	/// <summary>
	/// Every write succeeds fully or throws: EF has no partial-row upsert like the old Mongo layer
	/// </summary>
	private async Task SaveChangesAsync()
	{
		try
		{
			await _db.SaveChangesAsync();
		}
		catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
		{
			throw new StockItemCodeAlreadyExistsException();
		}
	}
}
