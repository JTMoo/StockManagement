using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IImportBatchServiceProvider
{
	/// <returns><see langword="null"/> if no batch has that id</returns>
	public Task<ImportBatch?> GetImportBatchAsync(string id, CancellationToken cancellationToken = default);

	public Task AddImportBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateImportBatchAsync(ImportBatch batch, CancellationToken cancellationToken = default);
}
