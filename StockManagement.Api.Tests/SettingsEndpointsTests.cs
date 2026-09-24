using System.Net;
using System.Net.Http.Json;
using StockManagement.Api.Features.Settings;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Tests;


[TestClass]
public sealed class SettingsEndpointsTests
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
	public async Task GetSettings_NothingStored_ReturnsGerman()
	{
		// Act
		var response = await _client.GetAsync("/api/settings");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		Assert.AreEqual(AvailableLanguages.German, (await response.Content.ReadAsAsync<SettingsResponse>()).Language);
	}

	[TestMethod]
	public async Task UpdateSettings_ValidRequest_PersistsAndReturnsIt()
	{
		// Act
		var response = await _client.PutAsJsonAsync("/api/settings", new UpdateSettingsRequest(AvailableLanguages.Spanish));

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		Assert.AreEqual(AvailableLanguages.Spanish, (await response.Content.ReadAsAsync<SettingsResponse>()).Language);
		Assert.AreEqual(AvailableLanguages.Spanish, (await (await _client.GetAsync("/api/settings")).Content.ReadAsAsync<SettingsResponse>()).Language);
	}

	[TestMethod]
	public async Task UpdateSettings_Twice_OverwritesLanguage()
	{
		// Arrange
		await _client.PutAsJsonAsync("/api/settings", new UpdateSettingsRequest(AvailableLanguages.Spanish));

		// Act
		var response = await _client.PutAsJsonAsync("/api/settings", new UpdateSettingsRequest(AvailableLanguages.English));

		// Assert
		Assert.AreEqual(AvailableLanguages.English, (await response.Content.ReadAsAsync<SettingsResponse>()).Language);
	}
}
