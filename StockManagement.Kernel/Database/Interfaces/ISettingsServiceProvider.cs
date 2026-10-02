using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface ISettingsServiceProvider
{
	/// <returns>The stored settings row, or <see langword="null"/> if none was stored yet</returns>
	public Task<AppSettings?> GetSettingsAsync(CancellationToken cancellationToken = default);

	public Task AddSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
