using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Customers;
using StockManagement.Api.Features.Users;
using StockManagement.Auth.Core.Contracts;

namespace StockManagement.Api.Tests;


/// <summary>
/// A Standard user only gets the permissions granted on their account (ADR-0017); #14's "simple users can't change client data"
/// </summary>
[TestClass]
public sealed class PermissionEnforcementTests
{
	private const string Password = "s3cret!23";

	private ApiFactory _factory;
	private HttpClient _adminClient;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_adminClient = await _factory.CreateAuthenticatedClientAsync();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_adminClient.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task StandardUser_WithoutCustomersWrite_CannotCreateCustomer()
	{
		// Arrange
		var client = await this.CreateStandardUserClientAsync("plain", []);

		// Act
		var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
	}

	[TestMethod]
	public async Task StandardUser_WithCustomersWrite_CanCreateCustomer()
	{
		// Arrange
		var client = await this.CreateStandardUserClientAsync("clerk", [Permission.CustomersWrite]);

		// Act
		var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest("Ana"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
	}

	[TestMethod]
	public async Task StandardUser_WithoutUsersManage_CannotListUsers()
	{
		// Arrange
		var client = await this.CreateStandardUserClientAsync("plain", []);

		// Act
		var response = await client.GetAsync("/api/users");

		// Assert
		Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
	}

	private async Task<HttpClient> CreateStandardUserClientAsync(string username, IReadOnlyList<string> permissions)
	{
		await _adminClient.PostAsJsonAsync("/api/users", new CreateUserRequest(username, Password, Permissions: permissions));
		return await _factory.CreateAuthenticatedClientAsync(username, Password);
	}
}
