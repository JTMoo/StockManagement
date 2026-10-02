using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Search;
using StockManagement.Api.Features.Users;
using StockManagement.Auth.Core.Contracts;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class SearchEndpointTests
{
	private const string Password = "s3cret!23";

	private ApiFactory _factory;
	private HttpClient _client;


	[TestInitialize]
	public async Task InitializeAsync()
	{
		_factory = new();
		_client = await _factory.CreateAuthenticatedClientAsync();

		await _client.PostAsJsonAsync("/api/stock-items", new { Code = "C-4521", Name = "Cemento Portland", Amount = 10 });
		await _client.PostAsJsonAsync("/api/customers", new { Name = "Ana", Lastname = "Gomez", IdentificationNumber = "4521234-5" });
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}


	[TestMethod]
	public async Task Search_ExactMatch_ReturnsHit()
	{
		// Act
		var response = await _client.GetAsync("/api/search?q=cemento");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var body = await response.Content.ReadAsAsync<SearchResponse>();
		var group = body.Groups.Single(g => g.Domain == "StockItems");
		CollectionAssert.AreEquivalent(new[] { "Cemento Portland" }, group.Items.Select(item => item.Title).ToList());
		Assert.AreEqual(1, group.TotalCount);
	}

	[TestMethod]
	public async Task Search_Typo_StillMatchesViaTrigram()
	{
		// Act
		var response = await _client.GetAsync("/api/search?q=sement");

		// Assert
		var body = await response.Content.ReadAsAsync<SearchResponse>();
		var group = body.Groups.Single(g => g.Domain == "StockItems");
		Assert.IsTrue(group.Items.Any(item => item.Title == "Cemento Portland"));
	}

	[TestMethod]
	public async Task Search_MatchesAcrossDomains()
	{
		// Act
		var response = await _client.GetAsync("/api/search?q=4521");

		// Assert
		var body = await response.Content.ReadAsAsync<SearchResponse>();
		Assert.IsTrue(body.Groups.Single(g => g.Domain == "StockItems").Items.Any(item => item.Title == "Cemento Portland"));
		Assert.IsTrue(body.Groups.Single(g => g.Domain == "Customers").Items.Any(item => item.Title == "Ana Gomez"));
	}

	[TestMethod]
	public async Task Search_EmptyQuery_ReturnsNoGroups()
	{
		// Act
		var response = await _client.GetAsync("/api/search?q=");

		// Assert
		var body = await response.Content.ReadAsAsync<SearchResponse>();
		Assert.AreEqual(0, body.Groups.Count);
	}

	[TestMethod]
	public async Task Search_StandardUserMissingPermission_OmitsThatDomainOnly()
	{
		// Arrange
		await _client.PostAsJsonAsync("/api/users", new CreateUserRequest("clerk", Password, Permissions: [Permission.StockItemsRead]));
		var client = await _factory.CreateAuthenticatedClientAsync("clerk", Password);

		// Act
		var response = await client.GetAsync("/api/search?q=cemento");

		// Assert
		var body = await response.Content.ReadAsAsync<SearchResponse>();
		CollectionAssert.AreEquivalent(new[] { "StockItems" }, body.Groups.Select(g => g.Domain).ToList());
	}
}
