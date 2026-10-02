using System.Net;
using Microsoft.Extensions.DependencyInjection;
using StockManagement.Api.Features.Sifen;
using StockManagement.Infrastructure.Database;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class SifenEndpointsTests
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
	public async Task ListStuckTransmissions_RejectedAndErrorInvoices_ReturnsBothExcludingPending()
	{
		// Arrange
		var db = _factory.ScopedServices.GetRequiredService<AppDbContext>();
		var customer = new Customer { CustomerId = 1001, Name = "Ann" };
		db.Customers.Add(customer);
		db.Invoices.AddRange(
			new Invoice { Number = "1", Customer = customer, Items = [], TransmissionStatus = TransmissionStatus.Pending },
			new Invoice { Number = "2", Customer = customer, Items = [], TransmissionStatus = TransmissionStatus.Rejected, Cdc = "cdc-2" },
			new Invoice { Number = "3", Customer = customer, Items = [], TransmissionStatus = TransmissionStatus.Error });
		await db.SaveChangesAsync();

		// Act
		var response = await _client.GetAsync("/api/sifen/stuck-transmissions");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var result = await response.Content.ReadAsAsync<StuckTransmissionListResponse>();
		CollectionAssert.AreEquivalent(new[] { "2", "3" }, result.Items.Select(item => item.Number).ToList());
		Assert.AreEqual("cdc-2", result.Items.Single(item => item.Number == "2").Cdc);
	}

	[TestMethod]
	public async Task ListStuckTransmissions_NoneStuck_ReturnsEmpty()
	{
		// Act
		var response = await _client.GetAsync("/api/sifen/stuck-transmissions");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var result = await response.Content.ReadAsAsync<StuckTransmissionListResponse>();
		Assert.AreEqual(0, result.Items.Count);
	}
}
