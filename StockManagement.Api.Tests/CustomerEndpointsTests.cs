using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Customers;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class CustomerEndpointsTests
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
	public async Task CreateCustomer_ValidRequest_Returns201WithFirstId()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana", Lastname: "Silva"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		Assert.AreEqual("/api/customers/1001", response.Headers.Location?.ToString());
		var customer = await response.Content.ReadAsAsync<CustomerResponse>();
		Assert.AreEqual(1001, customer.CustomerId);
		Assert.AreEqual("Silva", customer.Lastname);
	}

	[TestMethod]
	public async Task CreateCustomer_Twice_AssignsConsecutiveIdsAndListsBoth()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));
		await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Luis"));

		// Act
		var customers = await _client.GetFromJsonAsync<List<CustomerResponse>>("/api/customers", ApiFactory.JsonOptions);

		// Assert
		CollectionAssert.AreEquivalent(new[] { 1001, 1002 }, customers.Select(customer => customer.CustomerId).ToList());
	}

	[TestMethod]
	public async Task CreateCustomer_EmptyName_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest(""));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task GetCustomer_Created_ReturnsIt()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));

		// Act
		var response = await _client.GetAsync("/api/customers/1001");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		Assert.AreEqual("Ana", (await response.Content.ReadAsAsync<CustomerResponse>()).Name);
	}

	[TestMethod]
	public async Task GetCustomer_UnknownId_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/customers/4711");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateCustomer_Existing_ReturnsUpdatedAndPersists()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));

		// Act
		var response = await _client.PutAsJsonAsync("/api/customers/1001", new UpdateCustomerRequest(1001, "Ana", Lastname: "Silva"));

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		Assert.AreEqual("Silva", (await response.Content.ReadAsAsync<CustomerResponse>()).Lastname);
		Assert.AreEqual("Silva", (await (await _client.GetAsync("/api/customers/1001")).Content.ReadAsAsync<CustomerResponse>()).Lastname);
	}

	[TestMethod]
	public async Task UpdateCustomer_UnknownId_Returns404()
	{
		// Act
		var response = await _client.PutAsJsonAsync("/api/customers/4711", new UpdateCustomerRequest(4711, "Ana"));

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateCustomer_EmptyName_Returns400()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));

		// Act
		var response = await _client.PutAsJsonAsync("/api/customers/1001", new UpdateCustomerRequest(1001, ""));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
