using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Auth;
using StockManagement.Api.Features.Users;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class UserEndpointsTests
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
	public async Task CreateUser_ValidRequest_Returns201WithoutPasswordHash()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23", FullName: "Ana Gómez"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		var user = await response.Content.ReadAsAsync<UserResponse>();
		Assert.AreEqual("ana", user.Username);
		Assert.AreEqual("Ana Gómez", user.FullName);
		Assert.AreEqual(UserRole.Standard, user.Role);
	}

	[TestMethod]
	public async Task CreateUser_DuplicateUsername_Returns409()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23"));

		// Act
		var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "otherPass1!"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateUser_ShortPassword_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "short"));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task CreateUser_UnknownPermission_Returns400()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23", Permissions: ["NotARealPermission"]));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[TestMethod]
	public async Task ListUsers_IncludesSeededAdmin()
	{
		// Act
		var users = await _client.GetFromJsonAsync<List<UserResponse>>("/api/users", ApiFactory.JsonOptions);

		// Assert
		Assert.IsTrue(users.Any(user => user.Username == ApiFactory.SeededAdminUsername && user.Role == UserRole.Admin));
	}

	[TestMethod]
	public async Task GetUser_UnknownId_Returns404()
	{
		// Act
		var response = await _client.GetAsync("/api/users/00000000-0000-0000-0000-000000000099");

		// Assert
		Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateUser_Existing_ChangesProfileAndPermissions()
	{
		// Arrange
		var created = await (await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23"))).Content.ReadAsAsync<UserResponse>();

		// Act
		var response = await _client.PutAsJsonAsync($"/api/users/{created.Id}", new UpdateUserRequest(created.Id, "ana", Position: "Warehouse", Permissions: [Permission.StockItemsRead]));

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var updated = await response.Content.ReadAsAsync<UserResponse>();
		Assert.AreEqual("Warehouse", updated.Position);
		CollectionAssert.AreEqual(new[] { Permission.StockItemsRead }, updated.Permissions.ToList());
	}

	[TestMethod]
	public async Task UpdateUser_NewPassword_LogsInWithIt()
	{
		// Arrange
		var created = await (await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23"))).Content.ReadAsAsync<UserResponse>();
		await _client.PutAsJsonAsync($"/api/users/{created.Id}", new UpdateUserRequest(created.Id, "ana", Password: "newPassword1!"));

		// Act
		var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("ana", "newPassword1!"));

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}

	[TestMethod]
	public async Task DeleteUser_Existing_RemovesIt()
	{
		// Arrange
		var created = await (await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("ana", "s3cret!23"))).Content.ReadAsAsync<UserResponse>();

		// Act
		var response = await _client.DeleteAsync($"/api/users/{created.Id}");

		// Assert
		Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
		Assert.AreEqual(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/users/{created.Id}")).StatusCode);
	}

	[TestMethod]
	public async Task DeleteUser_Self_Returns409()
	{
		// Arrange
		var admin = (await _client.GetFromJsonAsync<List<UserResponse>>("/api/users", ApiFactory.JsonOptions)).Single(user => user.Username == ApiFactory.SeededAdminUsername);

		// Act
		var response = await _client.DeleteAsync($"/api/users/{admin.Id}");

		// Assert
		Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
	}
}
