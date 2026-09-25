using System.Net;
using System.Net.Http.Json;
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
		_client = await _factory.CreateAuthenticatedClientAsync();

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

	[TestMethod]
	public async Task CreateStockItem_NewCode_Returns201AndStores()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items", new { Code = "C3", Name = "Bolt", Amount = 4, Price = 250m });

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var stockItem = await response.Content.ReadAsAsync<StockItemResponse>();
		Assert.AreEqual("Bolt", stockItem.Name);
		Assert.AreEqual(4, stockItem.Amount);
		Assert.IsNotNull((await _client.GetAsync("/api/stock-items/C3")).Content);
	}

	[TestMethod]
	public async Task CreateStockItem_DuplicateCode_Returns409()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items", new { Code = "A1", Name = "Other" });

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateStockItem_EmptyName_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/stock-items", new { Code = "C3", Name = "" });

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateStockItem_KnownId_Returns200AndUpdates()
	{
		// Arrange
		var id = (await (await _client.GetAsync("/api/stock-items/A1")).Content.ReadAsAsync<StockItemResponse>()).Id;

		// Act
		var response = await _client.PutAsJsonAsync($"/api/stock-items/{id}", new { Id = id, Code = "A1", Name = "Screw XL", Amount = 20, Price = 6000m });

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var stockItem = await response.Content.ReadAsAsync<StockItemResponse>();
		Assert.AreEqual("Screw XL", stockItem.Name);
		Assert.AreEqual(20, stockItem.Amount);
	}

	[TestMethod]
	public async Task UpdateStockItem_CodeChangedToDuplicate_Returns409()
	{
		// Arrange
		var id = (await (await _client.GetAsync("/api/stock-items/A1")).Content.ReadAsAsync<StockItemResponse>()).Id;

		// Act
		var response = await _client.PutAsJsonAsync($"/api/stock-items/{id}", new { Id = id, Code = "B2", Name = "Screw" });

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateStockItem_UnknownId_Returns404()
	{
		// Act
		var response = await _client.PutAsJsonAsync("/api/stock-items/unknown", new { Id = "unknown", Code = "X9", Name = "Anything" });

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task DeleteStockItem_KnownId_Returns204AndRemoves()
	{
		// Arrange
		var id = (await (await _client.GetAsync("/api/stock-items/A1")).Content.ReadAsAsync<StockItemResponse>()).Id;

		// Act
		var response = await _client.DeleteAsync($"/api/stock-items/{id}");

		// Assert
		Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
		Assert.AreEqual(HttpStatusCode.NotFound, (await _client.GetAsync("/api/stock-items/A1")).StatusCode);
	}

	[TestMethod]
	public async Task DeleteStockItem_UnknownId_Returns404()
	{
		// Act
		var response = await _client.DeleteAsync("/api/stock-items/unknown");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}
}
