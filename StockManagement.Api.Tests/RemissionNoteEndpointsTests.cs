using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.RemissionNotes;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


/// <summary>
/// SIFEN Nota de Remisión Electrónica endpoints (#162) - a remission note documents a goods movement, it never
/// changes <see cref="StockItem.Amount"/> (unlike <see cref="SaleEndpointsTests"/>'s sale endpoint).
/// </summary>
[TestClass]
public sealed class RemissionNoteEndpointsTests
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
	public async Task CreateRemissionNote_ValidRequest_Returns201AndDoesNotChangeStock()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.TrasladoEntreLocales, "Avda. España 500", [new("A1", 3)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var remissionNote = await response.Content.ReadAsAsync<RemissionNoteResponse>();
		Assert.AreEqual("001-001-0000001", remissionNote.Number);
		Assert.AreEqual(1001, remissionNote.CustomerId);
		Assert.AreEqual(RemissionReason.TrasladoEntreLocales, remissionNote.Reason);
		Assert.AreEqual("Avda. España 500", remissionNote.DestinationAddress);
		Assert.AreEqual(3, remissionNote.Lines.Single().Amount);
		Assert.AreEqual(TransmissionStatus.Pending, remissionNote.TransmissionStatus);

		var stockItem = await _factory.ScopedServices.GetRequiredService<IStockItemServiceProvider>().GetStockItemAsync("A1");
		Assert.AreEqual(10, stockItem.Amount);
	}

	[TestMethod]
	public async Task CreateRemissionNote_Stored_CanBeReadBack()
	{
		// Arrange
		var created = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.Venta, "Avda. España 500", [new("A1", 1)]), ApiFactory.JsonOptions);

		// Act
		var response = await _client.GetAsync(created.Headers.Location);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var remissionNote = await response.Content.ReadAsAsync<RemissionNoteResponse>();
		Assert.AreEqual(RemissionReason.Venta, remissionNote.Reason);
		Assert.AreEqual("A1", remissionNote.Lines.Single().Code);
	}

	[TestMethod]
	public async Task ListRemissionNotes_OneStored_IncludesIt()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.Venta, "Avda. España 500", [new("A1", 1)]), ApiFactory.JsonOptions);

		// Act
		var response = await _client.GetFromJsonAsync<RemissionNoteListResponse>("/api/remission-notes", ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(1, response.Items.Count);
	}

	[TestMethod]
	public async Task CreateRemissionNote_UnknownCustomer_Returns404()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(4711, RemissionReason.Venta, "Avda. España 500", [new("A1", 1)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateRemissionNote_UnknownStockItemCode_Returns404()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.Venta, "Avda. España 500", [new("B2", 1)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateRemissionNote_NoItems_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.Venta, "Avda. España 500", []), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		StringAssert.Contains(await response.Content.ReadAsStringAsync(), "itemsRequired");
	}

	[TestMethod]
	public async Task CreateRemissionNote_EmptyDestinationAddress_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/remission-notes", new CreateRemissionNoteRequest(1001, RemissionReason.Venta, "", [new("A1", 1)]), ApiFactory.JsonOptions);

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
		StringAssert.Contains(await response.Content.ReadAsStringAsync(), "destinationAddressRequired");
	}

	[TestMethod]
	public async Task GetRemissionNote_UnknownNumber_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/remission-notes/99");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}
}
