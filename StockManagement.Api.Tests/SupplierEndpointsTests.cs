using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.Suppliers;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class SupplierEndpointsTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();

		var suppliers = _factory.ScopedServices.GetRequiredService<ISupplierServiceProvider>();
		await suppliers.AddSupplierAsync(new Supplier("Acme", country: "PY", currency: "PYG", leadTimeDays: 5));
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task ListSuppliers_OneStored_ReturnsIt()
	{
		// Act
		var response = await _client.GetAsync("/api/suppliers");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var suppliers = await response.Content.ReadAsAsync<SupplierListResponse>();
		CollectionAssert.AreEquivalent(new[] { "Acme" }, suppliers.Items.Select(supplier => supplier.Name).ToList());
	}

	[TestMethod]
	public async Task CreateSupplier_NewName_Returns201AndStores()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/suppliers", new { Name = "Globex", Country = "AR", Currency = "USD", LeadTimeDays = 10 });

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var supplier = await response.Content.ReadAsAsync<SupplierResponse>();
		Assert.AreEqual("Globex", supplier.Name);
		Assert.AreEqual(10, supplier.LeadTimeDays);
	}

	[TestMethod]
	public async Task CreateSupplier_DuplicateName_Returns409()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/suppliers", new { Name = "Acme" });

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateSupplier_EmptyName_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/suppliers", new { Name = "" });

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		StringAssert.Contains(await response.Content.ReadAsStringAsync(), "nameRequired");
	}

	[TestMethod]
	public async Task UpdateSupplier_KnownId_Returns200AndUpdates()
	{
		// Arrange
		var id = (await (await _client.GetAsync("/api/suppliers")).Content.ReadAsAsync<SupplierListResponse>()).Items.Single().Id;

		// Act
		var response = await _client.PutAsJsonAsync($"/api/suppliers/{id}", new { Id = id, Name = "Acme Corp", LeadTimeDays = 7 });

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var supplier = await response.Content.ReadAsAsync<SupplierResponse>();
		Assert.AreEqual("Acme Corp", supplier.Name);
		Assert.AreEqual(7, supplier.LeadTimeDays);
	}

	[TestMethod]
	public async Task UpdateSupplier_UnknownId_Returns404()
	{
		// Act
		var response = await _client.PutAsJsonAsync("/api/suppliers/unknown", new { Id = "unknown", Name = "Anything" });

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task DeleteSupplier_KnownId_Returns204AndRemoves()
	{
		// Arrange
		var id = (await (await _client.GetAsync("/api/suppliers")).Content.ReadAsAsync<SupplierListResponse>()).Items.Single().Id;

		// Act
		var response = await _client.DeleteAsync($"/api/suppliers/{id}");

		// Assert
		Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
		Assert.IsFalse((await (await _client.GetAsync("/api/suppliers")).Content.ReadAsAsync<SupplierListResponse>()).Items.Any());
	}

	[TestMethod]
	public async Task DeleteSupplier_AssignedToStockItem_Returns409()
	{
		// Arrange
		var supplierId = (await (await _client.GetAsync("/api/suppliers")).Content.ReadAsAsync<SupplierListResponse>()).Items.Single().Id;
		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "A1", Name = "Screw", SupplierId = supplierId });

		// Act
		var response = await _client.DeleteAsync($"/api/suppliers/{supplierId}");

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateStockItem_WithSupplierAndMinimumStock_ReturnedOnGet()
	{
		// Arrange
		var supplierId = (await (await _client.GetAsync("/api/suppliers")).Content.ReadAsAsync<SupplierListResponse>()).Items.Single().Id;
		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "A1", Name = "Screw", Amount = 1, SupplierId = supplierId, MinimumStock = 5 });

		// Act
		var response = await _client.GetAsync("/api/stock-items/A1");

		// Assert
		var stockItem = await response.Content.ReadAsAsync<Features.StockItems.StockItemResponse>();
		Assert.AreEqual(supplierId, stockItem.SupplierId);
		Assert.AreEqual("Acme", stockItem.SupplierName);
		Assert.AreEqual(5, stockItem.MinimumStock);
	}

	[TestMethod]
	public async Task ListStockItemsBelowMinimum_OnlyReturnsItemsBelowTheirMinimum()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "A1", Name = "Low", Amount = 1, MinimumStock = 5 });
		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "B2", Name = "Plenty", Amount = 10, MinimumStock = 5 });
		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "C3", Name = "NoMinimum", Amount = 0, MinimumStock = 0 });

		// Act
		var response = await _client.GetAsync("/api/stock-items/below-minimum");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var stockItems = await response.Content.ReadAsAsync<Features.StockItems.StockItemListResponse>();
		CollectionAssert.AreEquivalent(new[] { "A1" }, stockItems.Items.Select(item => item.Code).ToList());
	}
}
