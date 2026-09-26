using System.Net;
using System.Net.Http.Headers;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.Import;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class ImportBatchEndpointsTests
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
	public async Task Preview_NewCustomers_ReturnsReadyRowsAndStoresNothingYet()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers",
			["Name", "Lastname"],
			["Ann", "Miller"],
			["Bo", "Nolan"]));

		// Act
		var response = await _client.PostAsync("/api/import/batches", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var batch = await response.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(ImportBatchStatus.Previewed, batch.Status);
		Assert.AreEqual(2, batch.ReadyCount);
		Assert.AreEqual(0, batch.DuplicateCount);
		Assert.AreEqual(0, batch.ErrorCount);

		var customers = _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>();
		CollectionAssert.AreEqual(Array.Empty<Customer>(), (await customers.GetCustomersAsync()).ToList());
	}

	[TestMethod]
	public async Task Preview_IdentificationNumberAlreadyStored_ReportsAsDuplicate()
	{
		// Arrange
		var customers = _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>();
		await customers.AddCustomerAsync(new Customer { CustomerId = 1001, Name = "Ann", IdentificationNumber = "ID-1" });

		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name", "IdentificationNumber"], ["Ann again", "ID-1"]));

		// Act
		var response = await _client.PostAsync("/api/import/batches", content);

		// Assert
		var batch = await response.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(0, batch.ReadyCount);
		Assert.AreEqual(1, batch.DuplicateCount);
	}

	[TestMethod]
	public async Task PreviewThenCommit_ReadyCustomers_WritesThemAndAssignsCustomerIds()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name"], ["Ann"], ["Bo"]));
		var previewResponse = await _client.PostAsync("/api/import/batches", content);
		var previewed = await previewResponse.Content.ReadAsAsync<ImportBatchResponse>();

		// Act
		var commitResponse = await _client.PostAsync($"/api/import/batches/{previewed.Id}/commit", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, commitResponse.StatusCode);
		var committed = await commitResponse.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(ImportBatchStatus.Committed, committed.Status);

		var customers = _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>();
		var stored = (await customers.GetCustomersAsync()).OrderBy(customer => customer.CustomerId).ToList();
		CollectionAssert.AreEqual(new[] { "Ann", "Bo" }, stored.Select(customer => customer.Name).ToList());
		Assert.AreEqual(Customer.FirstCustomerId, stored[0].CustomerId);
		Assert.AreEqual(Customer.FirstCustomerId + 1, stored[1].CustomerId);
	}

	[TestMethod]
	public async Task Commit_AlreadyCommitted_Returns409()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name"], ["Ann"]));
		var previewed = await (await _client.PostAsync("/api/import/batches", content)).Content.ReadAsAsync<ImportBatchResponse>();
		await _client.PostAsync($"/api/import/batches/{previewed.Id}/commit", null);

		// Act
		var response = await _client.PostAsync($"/api/import/batches/{previewed.Id}/commit", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task Commit_UnknownBatch_Returns404()
	{
		// Act
		var response = await _client.PostAsync($"/api/import/batches/{Guid.NewGuid()}/commit", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task PreviewCommitThenUndo_RemovesTheImportedCustomers()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name"], ["Ann"]));
		var previewed = await (await _client.PostAsync("/api/import/batches", content)).Content.ReadAsAsync<ImportBatchResponse>();
		await _client.PostAsync($"/api/import/batches/{previewed.Id}/commit", null);

		// Act
		var undoResponse = await _client.PostAsync($"/api/import/batches/{previewed.Id}/undo", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, undoResponse.StatusCode);
		var undone = await undoResponse.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(ImportBatchStatus.Undone, undone.Status);

		var customers = _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>();
		CollectionAssert.AreEqual(Array.Empty<Customer>(), (await customers.GetCustomersAsync()).ToList());
	}

	[TestMethod]
	public async Task Undo_NotCommittedYet_Returns409()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name"], ["Ann"]));
		var previewed = await (await _client.PostAsync("/api/import/batches", content)).Content.ReadAsAsync<ImportBatchResponse>();

		// Act
		var response = await _client.PostAsync($"/api/import/batches/{previewed.Id}/undo", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task Get_UnknownBatch_Returns404()
	{
		// Act
		var response = await _client.GetAsync($"/api/import/batches/{Guid.NewGuid()}");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task Preview_StockItemsTarget_UsesTheSameGenericPipeline()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.StockItems, CreateWorkbook("Stock", ["Code", "Name", "Amount"], ["A1", "Screw", "10"]));

		// Act
		var previewResponse = await _client.PostAsync("/api/import/batches", content);
		var previewed = await previewResponse.Content.ReadAsAsync<ImportBatchResponse>();
		var commitResponse = await _client.PostAsync($"/api/import/batches/{previewed.Id}/commit", null);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, commitResponse.StatusCode);
		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		CollectionAssert.AreEqual(new[] { "A1" }, (await stockItems.GetAllStockItemsAsync()).Select(item => item.Code).ToList());
	}

	[TestMethod]
	public async Task Preview_StockItemCodeAlreadyStored_ReportsAsDuplicate()
	{
		// Arrange
		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		await stockItems.AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10));

		var content = ExcelFileContent(ImportTarget.StockItems, CreateWorkbook("Stock", ["Code", "Name"], ["A1", "Screw again"], ["B2", "Nut"]));

		// Act
		var response = await _client.PostAsync("/api/import/batches", content);

		// Assert
		var batch = await response.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(1, batch.ReadyCount);
		Assert.AreEqual(1, batch.DuplicateCount);
	}

	[TestMethod]
	public async Task Preview_StockItemRowWithBadNumber_ReportsRowErrorAndSkipsRow()
	{
		// Arrange
		var content = ExcelFileContent(ImportTarget.StockItems, CreateWorkbook("Stock",
			["Code", "Name", "Amount"],
			["A1", "Screw", "10"],
			["B2", "Nut", "not-a-number"]));

		// Act
		var response = await _client.PostAsync("/api/import/batches", content);

		// Assert
		var batch = await response.Content.ReadAsAsync<ImportBatchResponse>();
		Assert.AreEqual(1, batch.ReadyCount);
		Assert.AreEqual(1, batch.ErrorCount);
		Assert.AreEqual(3, batch.Rows.Single(row => row.Status == ImportRowStatus.Error).Row);
	}

	[TestMethod]
	public async Task Preview_NoFile_Returns400()
	{
		// Arrange
		using var content = new MultipartFormDataContent { { new StringContent(nameof(ImportTarget.Customers)), "Target" } };

		// Act
		var response = await _client.PostAsync("/api/import/batches", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task Preview_NoToken_ReturnsUnauthorized()
	{
		// Arrange
		using var anonymousClient = _factory.CreateClient();
		var content = ExcelFileContent(ImportTarget.Customers, CreateWorkbook("Customers", ["Name"], ["Ann"]));

		// Act
		var response = await anonymousClient.PostAsync("/api/import/batches", content);

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}


	private static MultipartFormDataContent ExcelFileContent(ImportTarget target, XLWorkbook workbook)
	{
		using var stream = new MemoryStream();
		workbook.SaveAs(stream);

		var content = new MultipartFormDataContent
		{
			{ new StringContent(target.ToString()), "Target" }
		};
		var fileContent = new ByteArrayContent(stream.ToArray());
		fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		content.Add(fileContent, "File", "legacy.xlsx");
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
