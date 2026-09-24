using Moq;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Settings.Core;

namespace StockManagement.Tests.Settings;


[TestClass]
public sealed class SettingsServiceTests
{
	private readonly Mock<ISettingsServiceProvider> _settings = new();


	[TestMethod]
	public async Task GetLanguageAsync_NothingStored_ReturnsGerman()
	{
		// Arrange
		_settings.Setup(provider => provider.GetSettingsAsync()).ReturnsAsync((AppSettings?)null);

		// Act
		var result = await this.CreateService().GetLanguageAsync();

		// Assert
		Assert.AreEqual(AvailableLanguages.German, result);
	}

	[TestMethod]
	public async Task GetLanguageAsync_Stored_ReturnsIt()
	{
		// Arrange
		_settings.Setup(provider => provider.GetSettingsAsync()).ReturnsAsync(new AppSettings { Language = AvailableLanguages.Spanish });

		// Act
		var result = await this.CreateService().GetLanguageAsync();

		// Assert
		Assert.AreEqual(AvailableLanguages.Spanish, result);
	}

	[TestMethod]
	public async Task SetLanguageAsync_NothingStored_Adds()
	{
		// Arrange
		_settings.Setup(provider => provider.GetSettingsAsync()).ReturnsAsync((AppSettings?)null);

		// Act
		await this.CreateService().SetLanguageAsync(AvailableLanguages.English);

		// Assert
		_settings.Verify(provider => provider.AddSettingsAsync(It.Is<AppSettings>(settings => settings.Language == AvailableLanguages.English)), Times.Once);
	}

	[TestMethod]
	public async Task SetLanguageAsync_AlreadyStored_UpdatesExisting()
	{
		// Arrange
		var stored = new AppSettings { Language = AvailableLanguages.German };
		_settings.Setup(provider => provider.GetSettingsAsync()).ReturnsAsync(stored);

		// Act
		await this.CreateService().SetLanguageAsync(AvailableLanguages.English);

		// Assert
		Assert.AreEqual(AvailableLanguages.English, stored.Language);
		_settings.Verify(provider => provider.UpdateSettingsAsync(stored), Times.Once);
		_settings.Verify(provider => provider.AddSettingsAsync(It.IsAny<AppSettings>()), Times.Never);
	}

	private SettingsService CreateService()
	{
		return new SettingsService(_settings.Object);
	}
}
