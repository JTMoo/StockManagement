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

	[TestMethod]
	public async Task GetCompanySettings_NothingStored_ReturnsDefaults()
	{
		// Act
		var response = await _client.GetAsync("/api/company-settings");

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var settings = await response.Content.ReadAsAsync<CompanySettingsResponse>();
		Assert.AreEqual(10m, settings.VatRatePercent);
		Assert.AreEqual(30, settings.PaymentTermInDays);
		Assert.AreEqual(1, settings.FirstInvoiceNumber);
		Assert.AreEqual(1001, settings.FirstCustomerId);
	}

	[TestMethod]
	public async Task UpdateCompanySettings_ValidRequest_PersistsAndReturnsIt()
	{
		// Arrange
		var request = new UpdateCompanySettingsRequest("Acme", "123456", "PYG", 5m, 14, 100, 2000, 0);

		// Act
		var response = await _client.PutAsJsonAsync("/api/company-settings", request);

		// Assert
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		var settings = await response.Content.ReadAsAsync<CompanySettingsResponse>();
		Assert.AreEqual("Acme", settings.CompanyName);
		Assert.AreEqual("123456", settings.TaxId);
		Assert.AreEqual("PYG", settings.Currency);
		Assert.AreEqual(5m, settings.VatRatePercent);
		Assert.AreEqual(14, settings.PaymentTermInDays);
		Assert.AreEqual(100, settings.FirstInvoiceNumber);
		Assert.AreEqual(2000, settings.FirstCustomerId);

		var stored = await (await _client.GetAsync("/api/company-settings")).Content.ReadAsAsync<CompanySettingsResponse>();
		Assert.AreEqual("Acme", stored.CompanyName);
	}

	[TestMethod]
	public async Task UpdateCompanySettings_VatRateOutOfRange_ReturnsBadRequest()
	{
		// Act
		var response = await _client.PutAsJsonAsync("/api/company-settings", new UpdateCompanySettingsRequest("Acme", "123456", "PYG", 150m, 14, 100, 2000, 0));

		// Assert
		Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
	}
}
