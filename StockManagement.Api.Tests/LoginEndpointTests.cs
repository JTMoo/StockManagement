using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Auth;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class LoginEndpointTests
{
	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public void Initialize()
	{
		_factory = new();
		_client = _factory.CreateClient();
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task Login_SeededAdminCredentials_ReturnsToken()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.SeededAdminUsername, ApiFactory.SeededAdminPassword));

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var body = await response.Content.ReadAsAsync<LoginResponse>();
		Assert.IsFalse(string.IsNullOrEmpty(body.Token));
		Assert.AreEqual(ApiFactory.SeededAdminUsername, body.Username);
	}

	[TestMethod]
	public async Task Login_WrongPassword_ReturnsUnauthorized()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.SeededAdminUsername, "WrongPassword1!"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[TestMethod]
	public async Task Login_UnknownUsername_ReturnsUnauthorized()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody", "WhateverPassword1!"));

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[TestMethod]
	public async Task Login_EmptyPassword_ReturnsBadRequest()
	{
		// Act
		var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.SeededAdminUsername, ""));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
