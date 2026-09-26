using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="IImportBatchServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfImportBatchServiceProvider(AppDbContext db) : IImportBatchServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<ImportBatch?> GetImportBatchAsync(string id)
	{
		return _db.ImportBatches.SingleOrDefaultAsync(batch => batch.Id == id);
	}

	public async Task AddImportBatchAsync(ImportBatch batch)
	{
		_db.ImportBatches.Add(batch);
		await _db.SaveChangesAsync();
	}

	public async Task<int> UpdateImportBatchAsync(ImportBatch batch)
	{
		_db.ImportBatches.Update(batch);
		await _db.SaveChangesAsync();
		return 1;
	}
}
