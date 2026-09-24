using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StockManagement.Api.Features.Invoices;
using StockManagement.Api.Features.Sales;
using StockManagement.Api.Features.StockItems;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class SaleEndpointsTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = _factory.CreateClient();

		await _factory.Services.GetRequiredService<IStockItemServiceProvider>().AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10, price: 5000));
		await _factory.Services.GetRequiredService<ICustomerServiceProvider>().AddCustomerAsync(new Customer() { CustomerId = 1001, Name = "Ana" });
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task CreateSale_EnoughStock_Returns201AndReducesStock()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 3)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var invoice = await response.Content.ReadAsAsync<InvoiceResponse>();
		Assert.AreEqual(1, invoice.Number);
		Assert.AreEqual(15000, invoice.Total);
		Assert.AreEqual(1364, invoice.Tax);
		Assert.AreEqual(1001, invoice.CustomerId);
		Assert.AreEqual(SaleCondition.Cash, invoice.SaleCondition);
		Assert.AreEqual(3, invoice.Lines.Single().Amount);
		var stockItem = await _client.GetFromJsonAsync<StockItemResponse>("/api/stock-items/A1", ApiFactory.JsonOptions);
		Assert.AreEqual(7, stockItem.Amount);
	}

	[TestMethod]
	public async Task CreateSale_Stored_CanBeReadBack()
	{
		// Arrange
		var created = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Credit, [new("A1", 1)]), ApiFactory.JsonOptions);

		// Act
		var response = await _client.GetAsync(created.Headers.Location);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var invoice = await response.Content.ReadAsAsync<InvoiceResponse>();
		Assert.AreEqual(SaleCondition.Credit, invoice.SaleCondition);
		Assert.AreEqual("A1", invoice.Lines.Single().Code);
	}

	[TestMethod]
	public async Task CreateSale_TooLittleStock_Returns409AndKeepsStock()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 11)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
		var conflict = await response.Content.ReadAsAsync<SaleConflictResponse>();
		CollectionAssert.AreEqual(new[] { "Screw" }, conflict.UnavailableItems.ToList());
		var stockItem = await _client.GetFromJsonAsync<StockItemResponse>("/api/stock-items/A1", ApiFactory.JsonOptions);
		Assert.AreEqual(10, stockItem.Amount);
	}

	[TestMethod]
	public async Task TryAddSaleAsync_SecondLineShort_WritesNothing()
	{
		// Arrange
		await _factory.Services.GetRequiredService<IStockItemServiceProvider>().AddStockItemAsync(new StockItem("Nut", code: "B2", amount: 1, price: 100));
		var invoice = CreateInvoice(1, ("A1", 3), ("B2", 2));

		// Act
		var result = await _factory.Services.GetRequiredService<IInvoiceServiceProvider>().TryAddSaleAsync(invoice);

		// Assert
		CollectionAssert.AreEqual(new[] { "B2" }, result.ToList());
		await this.AssertNothingSoldAsync();
	}

	[TestMethod]
	public async Task TryAddSaleAsync_InvoiceNumberTaken_ThrowsAndWritesNothing()
	{
		// Arrange
		var invoices = _factory.Services.GetRequiredService<IInvoiceServiceProvider>();
		await invoices.AddInvoiceAsync(new Invoice() { Number = 1, Items = [] });

		// Act
		await Assert.ThrowsExceptionAsync<MongoWriteException>(() => invoices.TryAddSaleAsync(CreateInvoice(1, ("A1", 3))));

		// Assert
		Assert.AreEqual(10, (await _factory.Services.GetRequiredService<IStockItemServiceProvider>().GetStockItemAsync("A1")).Amount);
	}

	[TestMethod]
	public async Task CreateSale_UnknownCustomer_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(4711, SaleCondition.Cash, [new("A1", 1)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		StringAssert.Contains(await response.Content.ReadAsStringAsync(), CreateSaleEndpoint.CustomerNotFound);
	}

	[TestMethod]
	public async Task CreateSale_NoItems_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, []), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateSale_ZeroAmount_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest(1001, SaleCondition.Cash, [new("A1", 0)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task GetInvoice_UnknownNumber_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/invoices/99");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	private async Task AssertNothingSoldAsync()
	{
		Assert.AreEqual(10, (await _factory.Services.GetRequiredService<IStockItemServiceProvider>().GetStockItemAsync("A1")).Amount);
		Assert.IsNull(await _factory.Services.GetRequiredService<IInvoiceServiceProvider>().GetInvoiceAync(1));
	}

	private static Invoice CreateInvoice(int number, params (string Code, int Amount)[] lines)
	{
		return new Invoice() { Number = number, Items = [.. lines.Select(line => new ShoppingCartItem(new StockItem(line.Code, code: line.Code, amount: 100)) { Amount = line.Amount })] };
	}
}
