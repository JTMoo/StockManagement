using System.Net;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.StockItems;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class StockItemEndpointsTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = _factory.CreateClient();

		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		await stockItems.AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10, price: 5000));
		await stockItems.AddStockItemAsync(new StockItem("Nut", code: "B2", amount: 3, price: 1000));
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task ListStockItems_TwoStored_ReturnsBoth()
	{
		// Act
		var response = await _client.GetAsync("/api/stock-items");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var stockItems = await response.Content.ReadAsAsync<List<StockItemResponse>>();
		CollectionAssert.AreEquivalent(new[] { "A1", "B2" }, stockItems.Select(item => item.Code).ToList());
	}

	[TestMethod]
	public async Task GetStockItem_KnownCode_ReturnsItem()
	{
		// Act
		var response = await _client.GetAsync("/api/stock-items/A1");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var stockItem = await response.Content.ReadAsAsync<StockItemResponse>();
		Assert.AreEqual("Screw", stockItem.Name);
		Assert.AreEqual(10, stockItem.Amount);
		Assert.AreEqual(5000m, stockItem.Price);
	}

	[TestMethod]
	public async Task GetStockItem_UnknownCode_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/stock-items/X9");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}
}
