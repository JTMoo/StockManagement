using Microsoft.EntityFrameworkCore;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Infrastructure.Database;


/// <summary>
/// <see cref="ISettingsServiceProvider"/> on <see cref="AppDbContext"/>
/// </summary>
public class EfSettingsServiceProvider(AppDbContext db) : ISettingsServiceProvider
{
	private readonly AppDbContext _db = db;


	public Task<AppSettings?> GetSettingsAsync()
	{
		return _db.AppSettings.FirstOrDefaultAsync();
	}

	public async Task AddSettingsAsync(AppSettings settings)
	{
		_db.AppSettings.Add(settings);
		await _db.SaveChangesAsync();
	}

	public async Task<int> UpdateSettingsAsync(AppSettings settings)
	{
		_db.AppSettings.Update(settings);
		await _db.SaveChangesAsync();
		return 1;
	}
}
