using ClosedXML.Excel;
using StockManagement.Import.Core;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class ExcelStockItemParserTests
{
	private readonly ExcelStockItemParser _parser = new();


	[TestMethod]
	public async Task ParseAsync_HeadersMatchPropertyNames_ReadsItems()
	{
		// Arrange
		using var workbook = CreateWorkbook("Stock",
			["Code", "Name", "Amount", "Price"],
			["A1", "Screw", "10", "5.5"],
			["B2", "Nut", "3", "1"]);

		// Act
		var result = await _parser.ParseAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual("Stock", result.SheetName);
		Assert.AreEqual(0, result.Errors.Count);
		CollectionAssert.AreEqual(new[] { "A1", "B2" }, result.Items.Select(item => item.Code).ToList());
		Assert.AreEqual(10, result.Items[0].Amount);
		Assert.AreEqual(5.5m, result.Items[0].Price);
	}

	[TestMethod]
	public async Task ParseAsync_HeaderIsLocalizedDisplayName_MatchesProperty()
	{
		// Arrange: German header matches StockItem.Code's Display(Name = "code") resource
		using var workbook = CreateWorkbook("Stock", ["code", "name"], ["A1", "Screw"]);

		// Act
		var result = await _parser.ParseAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual(1, result.Items.Count);
		Assert.AreEqual("A1", result.Items[0].Code);
	}

	[TestMethod]
	public async Task ParseAsync_UnknownHeader_ColumnIgnored()
	{
		// Arrange
		using var workbook = CreateWorkbook("Stock", ["Code", "Unknown"], ["A1", "whatever"]);

		// Act
		var result = await _parser.ParseAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual(1, result.Items.Count);
		Assert.AreEqual("A1", result.Items[0].Code);
	}

	[TestMethod]
	public async Task ParseAsync_RowWithUnconvertibleValue_ReportsRowErrorAndSkipsRow()
	{
		// Arrange
		using var workbook = CreateWorkbook("Stock",
			["Code", "Name", "Amount"],
			["A1", "Screw", "10"],
			["B2", "Nut", "not-a-number"]);

		// Act
		var result = await _parser.ParseAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual(1, result.Items.Count);
		Assert.AreEqual("A1", result.Items[0].Code);
		Assert.AreEqual(1, result.Errors.Count);
		Assert.AreEqual(3, result.Errors[0].Row);
	}

	[TestMethod]
	public async Task ParseAsync_EmptySheet_ReturnsNoItems()
	{
		// Arrange
		using var workbook = new XLWorkbook();
		workbook.AddWorksheet("Stock");

		// Act
		var result = await _parser.ParseAsync(ToStream(workbook));

		// Assert
		Assert.AreEqual(0, result.Items.Count);
		Assert.AreEqual(0, result.Errors.Count);
	}

	private static XLWorkbook CreateWorkbook(string sheetName, string[] headers, params string[][] rows)
	{
		var workbook = new XLWorkbook();
		var worksheet = workbook.AddWorksheet(sheetName);

		for (var column = 0; column < headers.Length; column++)
		{
			worksheet.Cell(1, column + 1).Value = headers[column];
		}

		for (var row = 0; row < rows.Length; row++)
		{
			for (var column = 0; column < rows[row].Length; column++)
			{
				worksheet.Cell(row + 2, column + 1).Value = rows[row][column];
			}
		}

		return workbook;
	}

	private static MemoryStream ToStream(XLWorkbook workbook)
	{
		var stream = new MemoryStream();
		workbook.SaveAs(stream);
		stream.Position = 0;
		return stream;
	}
}
