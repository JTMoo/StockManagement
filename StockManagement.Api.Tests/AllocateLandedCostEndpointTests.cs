using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.StockItems;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class AllocateLandedCostEndpointTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();

		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		await stockItems.AddStockItemAsync(new StockItem("Screw", code: "A1", amount: 10) { PurchasePrice = 10m, PurchaseExchangeRate = 1m });
		await stockItems.AddStockItemAsync(new StockItem("Nut", code: "B2", amount: 30) { PurchasePrice = 10m, PurchaseExchangeRate = 1m });
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task AllocateLandedCost_TwoItems_SplitsProportionalToPurchaseValueAndUpdatesAdditionalCost()
	{
		// Arrange
		var itemA = await (await _client.GetAsync("/api/stock-items/A1")).Content.ReadAsAsync<StockItemResponse>();
		var itemB = await (await _client.GetAsync("/api/stock-items/B2")).Content.ReadAsAsync<StockItemResponse>();

		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items/landed-cost/allocate", new { StockItemIds = new[] { itemA.Id, itemB.Id }, FreightCost = 300m, BrokerFee = 100m });

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var allocations = await response.Content.ReadAsAsync<List<LandedCostAllocationResponse>>();

		var allocationA = allocations.Single(a => a.StockItemId == itemA.Id);
		var allocationB = allocations.Single(a => a.StockItemId == itemB.Id);
		Assert.AreEqual(100m, allocationA.AllocatedCost); // value 100 of total value 400 -> 1/4 of 400 total cost
		Assert.AreEqual(10m, allocationA.AdditionalPurchaseCost); // 100 / 10 units
		Assert.AreEqual(300m, allocationB.AllocatedCost);
		Assert.AreEqual(10m, allocationB.AdditionalPurchaseCost); // 300 / 30 units

		var stored = await (await _client.GetAsync("/api/stock-items/A1")).Content.ReadAsAsync<StockItemResponse>();
		Assert.AreEqual(10m, stored.AdditionalPurchaseCost);
	}

	[TestMethod]
	public async Task AllocateLandedCost_UnknownStockItemId_Returns404()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items/landed-cost/allocate", new { StockItemIds = new[] { "missing" }, FreightCost = 100m });

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task AllocateLandedCost_NoPurchaseValueOnAnyItem_Returns400()
	{
		// Arrange
		var stockItems = _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>();
		await stockItems.AddStockItemAsync(new StockItem("NoPurchaseData", code: "C3", amount: 5));
		var itemC = await (await _client.GetAsync("/api/stock-items/C3")).Content.ReadAsAsync<StockItemResponse>();

		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items/landed-cost/allocate", new { StockItemIds = new[] { itemC.Id }, FreightCost = 100m });

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
