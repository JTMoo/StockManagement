using System.Net;

namespace StockManagement.Api.Tests;


/// <summary>
/// Endpoints other than login require a bearer token
/// </summary>
[TestClass]
public sealed class AuthorizationTests
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
	public async Task GetStockItems_NoToken_ReturnsUnauthorized()
	{
		// Act
		var response = await _client.GetAsync("/api/stock-items");

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[TestMethod]
	public async Task GetSettings_NoToken_ReturnsUnauthorized()
	{
		// Act
		var response = await _client.GetAsync("/api/settings");

		// Assert
		Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}
