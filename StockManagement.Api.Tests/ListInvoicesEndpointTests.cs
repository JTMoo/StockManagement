using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.Invoices;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class ListInvoicesEndpointTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = _factory.CreateClient();

		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		var customers = _factory.ScopedServices.GetRequiredService<ICustomerServiceProvider>();
		var invoices = _factory.ScopedServices.GetRequiredService<IInvoiceServiceProvider>();

		await stockItems.AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10, price: 5000));
		await customers.AddCustomerAsync(new Customer { CustomerId = 1001, Name = "Ana", Lastname = "Gómez" });
		await customers.AddCustomerAsync(new Customer { CustomerId = 1002, Name = "Bo" });
		var ana = await customers.GetCustomerAsync(1001);
		var bo = await customers.GetCustomerAsync(1002);

		await invoices.AddInvoiceAsync(new Invoice { Number = 1, Customer = ana, Date = new DateTime(2026, 1, 1), SaleCondition = SaleCondition.Cash, Items = [] });
		await invoices.AddInvoiceAsync(new Invoice { Number = 2, Customer = bo, Date = new DateTime(2026, 2, 1), SaleCondition = SaleCondition.Cash, Items = [] });
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task ListInvoices_NoFilter_ReturnsBothNewestFirstWithCustomerName()
	{
		// Act
		var response = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(2, response!.TotalCount);
		Assert.AreEqual(2, response.Items[0].Number);
		Assert.AreEqual(1, response.Items[1].Number);
		Assert.AreEqual("Ana Gómez", response.Items[1].CustomerName);
	}

	[TestMethod]
	public async Task ListInvoices_FilteredByCustomerId_ReturnsOnlyThatCustomer()
	{
		// Act
		var response = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices?customerId=1002", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(1, response!.TotalCount);
		Assert.AreEqual(2, response.Items.Single().Number);
	}

	[TestMethod]
	public async Task ListInvoices_FilteredByDateRange_ExcludesOutsideRange()
	{
		// Act
		var response = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices?from=2026-01-15&to=2026-12-31", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(1, response!.TotalCount);
		Assert.AreEqual(2, response.Items.Single().Number);
	}

	[TestMethod]
	public async Task ListInvoices_PageSizeOne_PagesAcrossBothInvoices()
	{
		// Act
		var first = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices?page=1&pageSize=1", ApiFactory.JsonOptions);
		var second = await _client.GetFromJsonAsync<InvoiceListResponse>("/api/invoices?page=2&pageSize=1", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(2, first!.TotalCount);
		Assert.AreEqual(2, first.Items.Single().Number);
		Assert.AreEqual(2, second!.TotalCount);
		Assert.AreEqual(1, second.Items.Single().Number);
	}
}
