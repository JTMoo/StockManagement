using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IImportBatchServiceProvider
{
	/// <returns><see langword="null"/> if no batch has that id</returns>
	public Task<ImportBatch?> GetImportBatchAsync(string id);

	public Task AddImportBatchAsync(ImportBatch batch);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateImportBatchAsync(ImportBatch batch);
}
