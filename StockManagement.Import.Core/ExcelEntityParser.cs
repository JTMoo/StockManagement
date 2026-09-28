using System.Reflection;
using ClosedXML.Excel;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Exceptions;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Import.Core;


/// <summary>
/// Reads <typeparamref name="T"/> instances from the first worksheet of an Excel file, matching columns to its writable properties by name
/// </summary>
/// <remarks>Shared by every <see cref="IImportTargetHandler"/>.</remarks>
internal static class ExcelEntityParser<T> where T : new()
{
	private static readonly IReadOnlyList<PropertyInfo> ImportableProperties =
		[.. typeof(T).GetProperties().Where(property => property.DeclaringType == typeof(T) && property.CanWrite)];


	/// <returns>The target's importable fields, by their C# property <c>Name</c> and localized <c>[Display]</c> value</returns>
	public static IReadOnlyList<ImportField> GetFields() =>
		[.. ImportableProperties.Select(property => new ImportField(property.Name, property.GetDisplayValue()))];

	/// <returns>Every column of the first worksheet's header row, 1-based, with the field <c>Name</c> it auto-matches to (or <see langword="null"/>)</returns>
	public static Task<(string SheetName, IReadOnlyList<DetectedColumn> Columns)> DetectColumnsAsync(Stream excelFile, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.Run(() => DetectColumns(excelFile), cancellationToken);
	}

	/// <returns>Successfully parsed items with their 1-based source row number, and one error per row that failed to convert</returns>
	/// <param name="columnMapping">Column number → target field <c>Name</c>; <see langword="null"/> auto-matches headers by name instead</param>
	public static Task<(string SheetName, IReadOnlyList<(int Row, T Item)> Items, IReadOnlyList<ImportRowError> Errors)> ParseAsync(Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.Run(() => Parse(excelFile, columnMapping), cancellationToken);
	}

	private static (string, IReadOnlyList<DetectedColumn>) DetectColumns(Stream excelFile)
	{
		using var workbook = new XLWorkbook(excelFile);
		var worksheet = workbook.Worksheets.First();

		if (worksheet.FirstRowUsed() is not IXLRow headerRow) return (worksheet.Name, []);

		List<DetectedColumn> columns = [];
		var column = 0;
		foreach (var cell in headerRow.RowUsed().Cells())
		{
			column++;
			var header = cell.GetString().Trim();
			columns.Add(new DetectedColumn(column, header, MatchHeader(header)?.Name));
		}

		return (worksheet.Name, columns);
	}

	private static (string, IReadOnlyList<(int, T)>, IReadOnlyList<ImportRowError>) Parse(Stream excelFile, IReadOnlyDictionary<int, string>? columnMapping)
	{
		using var workbook = new XLWorkbook(excelFile);
		var worksheet = workbook.Worksheets.First();

		if (worksheet.FirstRowUsed() is not IXLRow headerRow) return (worksheet.Name, [], []);

		var matches = columnMapping is null ? MatchHeadersToProperties(headerRow.RowUsed()) : MatchColumnsToProperties(columnMapping);

		List<(int, T)> items = [];
		List<ImportRowError> errors = [];
		var currentRow = headerRow.RowUsed().RowBelow();
		while (!currentRow.IsEmpty())
		{
			try
			{
				items.Add((currentRow.RowNumber(), CreateEntity(matches, currentRow)));
			}
			catch (FailedExcelConversionException ex)
			{
				errors.Add(new ImportRowError(currentRow.RowNumber(), ex.Message));
			}

			currentRow = currentRow.RowBelow();
		}

		return (worksheet.Name, items, errors);
	}

	private static T CreateEntity(IReadOnlyList<(PropertyInfo Property, int Column)> matches, IXLRangeRow row)
	{
		var entity = new T();
		foreach (var (property, column) in matches)
		{
			if (!row.Cell(column).TryGetValue(out string cellValue) || string.IsNullOrWhiteSpace(cellValue)) continue;

			try
			{
				if (property.PropertyType.TryConvertFromString(cellValue, out var value))
				{
					property.SetValue(entity, value);
				}
			}
			catch (ArgumentException argumentException)
			{
				throw new FailedExcelConversionException(argumentException, column);
			}
		}

		return entity;
	}

	private static IReadOnlyList<(PropertyInfo, int)> MatchHeadersToProperties(IXLRangeRow headerRow)
	{
		List<(PropertyInfo, int)> matches = [];

		var column = 0;
		foreach (var cell in headerRow.Cells())
		{
			column++;
			var property = MatchHeader(cell.GetString().Trim());
			if (property is not null) matches.Add((property, column));
		}

		return matches;
	}

	private static IReadOnlyList<(PropertyInfo, int)> MatchColumnsToProperties(IReadOnlyDictionary<int, string> columnMapping)
	{
		List<(PropertyInfo, int)> matches = [];

		foreach (var (column, fieldName) in columnMapping)
		{
			var property = ImportableProperties.FirstOrDefault(property => string.Equals(property.Name, fieldName, StringComparison.OrdinalIgnoreCase));
			if (property is not null) matches.Add((property, column));
		}

		return matches;
	}

	private static PropertyInfo? MatchHeader(string header) => ImportableProperties.FirstOrDefault(property =>
		string.Equals(property.Name, header, StringComparison.OrdinalIgnoreCase) ||
		string.Equals(property.GetDisplayValue(), header, StringComparison.OrdinalIgnoreCase));
}
