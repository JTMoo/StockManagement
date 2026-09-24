using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Settings.Core;


internal class SettingsService(ISettingsServiceProvider settingsServiceProvider) : ISettingsService
{
	private readonly ISettingsServiceProvider _settingsServiceProvider = settingsServiceProvider;


	public async Task<AvailableLanguages> GetLanguageAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var settings = await _settingsServiceProvider.GetSettingsAsync();
		return settings?.Language ?? AvailableLanguages.German;
	}

	public async Task SetLanguageAsync(AvailableLanguages language, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (await _settingsServiceProvider.GetSettingsAsync() is AppSettings settings)
		{
			settings.Language = language;
			await _settingsServiceProvider.UpdateSettingsAsync(settings);
		}
		else
		{
			await _settingsServiceProvider.AddSettingsAsync(new AppSettings { Language = language });
		}
	}
}
