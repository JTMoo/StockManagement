using System.Net;
using System.Net.Http.Headers;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.StockItems;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class StockItemImportEndpointTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task Import_NewItems_Returns200AndStoresThem()
	{
		// Arrange
		var content = ExcelFileContent(CreateWorkbook("Stock",
			["Code", "Name", "Amount", "Price"],
			["A1", "Screw", "10", "5.5"],
			["B2", "Nut", "3", "1"]));

		// Act
		var response = await _client.PostAsync("/api/stock-items/import", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var result = await response.Content.ReadAsAsync<StockItemImportResponse>();
		Assert.AreEqual(2, result.Imported);
		Assert.AreEqual(0, result.Duplicates);
		Assert.AreEqual(0, result.Errors.Count);

		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		CollectionAssert.AreEquivalent(new[] { "A1", "B2" }, (await stockItems.GetAllStockItemsAsync()).Select(item => item.Code).ToList());
	}

	[TestMethod]
	public async Task Import_CodeAlreadyStored_ReportsAsDuplicateAndSkipsIt()
	{
		// Arrange
		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		await stockItems.AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10));

		var content = ExcelFileContent(CreateWorkbook("Stock", ["Code", "Name"], ["A1", "Screw again"], ["B2", "Nut"]));

		// Act
		var response = await _client.PostAsync("/api/stock-items/import", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var result = await response.Content.ReadAsAsync<StockItemImportResponse>();
		Assert.AreEqual(1, result.Imported);
		Assert.AreEqual(1, result.Duplicates);
	}

	[TestMethod]
	public async Task Import_RowWithBadNumber_ReportsRowErrorAndSkipsRow()
	{
		// Arrange
		var content = ExcelFileContent(CreateWorkbook("Stock",
			["Code", "Name", "Amount"],
			["A1", "Screw", "10"],
			["B2", "Nut", "not-a-number"]));

		// Act
		var response = await _client.PostAsync("/api/stock-items/import", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var result = await response.Content.ReadAsAsync<StockItemImportResponse>();
		Assert.AreEqual(1, result.Imported);
		Assert.AreEqual(1, result.Errors.Count);
		Assert.AreEqual(3, result.Errors[0].Row);
	}

	[TestMethod]
	public async Task Import_NoFile_Returns400()
	{
		// Act
		var response = await _client.PostAsync("/api/stock-items/import", new MultipartFormDataContent());

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task Import_NotAnExcelFile_Returns400()
	{
		// Arrange
		using var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent([1, 2, 3, 4]);
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
		content.Add(fileContent, "File", "stock.xlsx");

		// Act
		var response = await _client.PostAsync("/api/stock-items/import", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task Import_NoToken_ReturnsUnauthorized()
	{
		// Arrange
		using var anonymousClient = _factory.CreateClient();
		var content = ExcelFileContent(CreateWorkbook("Stock", ["Code", "Name"], ["A1", "Screw"]));

		// Act
		var response = await anonymousClient.PostAsync("/api/stock-items/import", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}


	private static MultipartFormDataContent ExcelFileContent(XLWorkbook workbook)
	{
		using var stream = new MemoryStream();
		workbook.SaveAs(stream);

		var content = new MultipartFormDataContent();
		var fileContent = new ByteArrayContent(stream.ToArray());
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		content.Add(fileContent, "File", "stock.xlsx");
		return content;
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
}
