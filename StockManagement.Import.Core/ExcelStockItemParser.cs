using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Model;

namespace StockManagement.Import.Core;


internal sealed class ExcelStockItemParser : IExcelStockItemParser
{
	public async Task<ExcelParseResult> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		var (sheetName, items, errors) = await ExcelEntityParser<StockItem>.ParseAsync(excelFile, cancellationToken);
		return new(sheetName, [.. items.Select(item => item.Item)], [.. errors.Select(error => new StockItemImportRowError(error.Row, error.Message))]);
	}
}
