using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.CreditNotes;
using StockManagement.Api.Features.Invoices;
using StockManagement.Api.Features.Sales;
using StockManagement.Api.Features.StockItems;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class CreditNoteEndpointsTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();

		await _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>().AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10, price: 5000));
		await _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>().AddCustomerAsync(new Customer() { CustomerId = 1001, Name = "Ana" });
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task CancelInvoice_NotYetCancelled_Returns200AndRestocksAndStoresCreditNote()
	{
		// Arrange
		var sale = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 3)]), ApiFactory.JsonOptions);
		var invoice = await sale.Content.ReadAsAsync<InvoiceResponse>();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Number}/cancel", new CancelInvoiceRequest(invoice.Number, "Customer returned the goods"), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var creditNote = await response.Content.ReadAsAsync<CreditNoteResponse>();
		Assert.AreEqual(1, creditNote.Number);
		Assert.AreEqual(invoice.Number, creditNote.InvoiceNumber);
		Assert.AreEqual(invoice.Total, creditNote.Total);
		Assert.AreEqual("Customer returned the goods", creditNote.Reason);

		var stockItem = await _client.GetFromJsonAsync<StockItemResponse>("/api/stock-items/A1", ApiFactory.JsonOptions);
		Assert.AreEqual(10, stockItem.Amount);

		var storedInvoice = await _client.GetFromJsonAsync<InvoiceResponse>($"/api/invoices/{invoice.Number}", ApiFactory.JsonOptions);
		Assert.IsTrue(storedInvoice.IsCancelled);
	}

	[TestMethod]
	public async Task CancelInvoice_AlreadyCancelled_Returns409AndWritesNothing()
	{
		// Arrange
		var sale = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 3)]), ApiFactory.JsonOptions);
		var invoice = await sale.Content.ReadAsAsync<InvoiceResponse>();
		await _client.PostAsJsonAsync($"/api/invoices/{invoice.Number}/cancel", new CancelInvoiceRequest(invoice.Number, "First cancellation"), ApiFactory.JsonOptions);

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Number}/cancel", new CancelInvoiceRequest(invoice.Number, "Second cancellation"), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
		var stockItem = await _client.GetFromJsonAsync<StockItemResponse>("/api/stock-items/A1", ApiFactory.JsonOptions);
		Assert.AreEqual(10, stockItem.Amount);
	}

	[TestMethod]
	public async Task CancelInvoice_UnknownNumber_Returns404()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/invoices/99/cancel", new CancelInvoiceRequest("99", "Any reason"), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task CancelInvoice_EmptyReason_Returns400()
	{
		// Arrange
		var sale = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 3)]), ApiFactory.JsonOptions);
		var invoice = await sale.Content.ReadAsAsync<InvoiceResponse>();

		// Act
		var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Number}/cancel", new CancelInvoiceRequest(invoice.Number, ""), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		StringAssert.Contains(await response.Content.ReadAsStringAsync(), "reasonRequired");
	}

	[TestMethod]
	public async Task GetCreditNote_Stored_CanBeReadBack()
	{
		// Arrange
		var sale = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 3)]), ApiFactory.JsonOptions);
		var invoice = await sale.Content.ReadAsAsync<InvoiceResponse>();
		await _client.PostAsJsonAsync($"/api/invoices/{invoice.Number}/cancel", new CancelInvoiceRequest(invoice.Number, "Customer returned the goods"), ApiFactory.JsonOptions);

		// Act
		var response = await _client.GetAsync("/api/credit-notes/1");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var creditNote = await response.Content.ReadAsAsync<CreditNoteResponse>();
		Assert.AreEqual(invoice.Number, creditNote.InvoiceNumber);
	}

	[TestMethod]
	public async Task GetCreditNote_UnknownNumber_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/credit-notes/99");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}
}
