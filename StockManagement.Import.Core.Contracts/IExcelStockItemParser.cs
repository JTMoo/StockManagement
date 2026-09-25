namespace StockManagement.Import.Core.Contracts;


public interface IExcelStockItemParser
{
	/// <summary>
	/// Reads stock items from the first worksheet of an Excel file, matching columns to <see cref="Kernel.Model.StockItem"/> properties by header name
	/// </summary>
	public Task<ExcelParseResult> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default);
}
