using System.Reflection;
using ClosedXML.Excel;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Import.Core;


internal sealed class ExcelStockItemParser : IExcelStockItemParser
{
	private static readonly IReadOnlyList<PropertyInfo> ImportableProperties =
		[.. typeof(StockItem).GetProperties().Where(property => property.DeclaringType == typeof(StockItem) && property.CanWrite)];


	public Task<ExcelParseResult> ParseAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.Run(() => Parse(excelFile), cancellationToken);
	}

	private static ExcelParseResult Parse(Stream excelFile)
	{
		using var workbook = new XLWorkbook(excelFile);
		var worksheet = workbook.Worksheets.First();

		if (worksheet.FirstRowUsed() is not IXLRow headerRow) return new(worksheet.Name, [], []);

		var matches = MatchHeadersToProperties(headerRow.RowUsed());

		List<StockItem> items = [];
		List<StockItemImportRowError> errors = [];
		var currentRow = headerRow.RowUsed().RowBelow();
		while (!currentRow.IsEmpty())
		{
			try
			{
				items.Add(CreateStockItem(matches, currentRow));
			}
			catch (FailedExcelConversionException ex)
			{
				errors.Add(new StockItemImportRowError(currentRow.RowNumber(), ex.Message));
			}

			currentRow = currentRow.RowBelow();
		}

		return new(worksheet.Name, items, errors);
	}

	private static StockItem CreateStockItem(IReadOnlyList<(PropertyInfo Property, int Column)> matches, IXLRangeRow row)
	{
		var stockItem = new StockItem();
		foreach (var (property, column) in matches)
		{
			if (!row.Cell(column).TryGetValue(out string cellValue) || string.IsNullOrWhiteSpace(cellValue)) continue;

			try
			{
				if (property.PropertyType.TryConvertFromString(cellValue, out var value))
				{
					property.SetValue(stockItem, value);
				}
			}
			catch (ArgumentException argumentException)
			{
				throw new FailedExcelConversionException(argumentException, column);
			}
		}

		return stockItem;
	}

	private static IReadOnlyList<(PropertyInfo Property, int Column)> MatchHeadersToProperties(IXLRangeRow headerRow)
	{
		List<(PropertyInfo, int)> matches = [];

		var column = 0;
		foreach (var cell in headerRow.Cells())
		{
			column++;
			var header = cell.GetString().Trim();
			var property = ImportableProperties.FirstOrDefault(property =>
				string.Equals(property.Name, header, StringComparison.OrdinalIgnoreCase) ||
				string.Equals(property.GetDisplayValue(), header, StringComparison.OrdinalIgnoreCase));

			if (property is not null) matches.Add((property, column));
		}

		return matches;
	}
}
