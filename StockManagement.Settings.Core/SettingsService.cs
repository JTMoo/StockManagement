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

	public async Task<CompanySettings> GetCompanySettingsAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var settings = await _settingsServiceProvider.GetSettingsAsync() ?? new AppSettings();
		return new CompanySettings(settings.CompanyName, settings.TaxId, settings.Currency, settings.VatRatePercent, settings.PaymentTermInDays, settings.FirstInvoiceNumber, settings.FirstCustomerId, settings.CurrencyDecimalDigits);
	}

	public async Task SetCompanySettingsAsync(CompanySettings companySettings, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		ArgumentNullException.ThrowIfNull(companySettings);

		if (await _settingsServiceProvider.GetSettingsAsync() is AppSettings settings)
		{
			settings.CompanyName = companySettings.CompanyName;
			settings.TaxId = companySettings.TaxId;
			settings.Currency = companySettings.Currency;
			settings.VatRatePercent = companySettings.VatRatePercent;
			settings.PaymentTermInDays = companySettings.PaymentTermInDays;
			settings.FirstInvoiceNumber = companySettings.FirstInvoiceNumber;
			settings.FirstCustomerId = companySettings.FirstCustomerId;
			settings.CurrencyDecimalDigits = companySettings.CurrencyDecimalDigits;
			await _settingsServiceProvider.UpdateSettingsAsync(settings);
		}
		else
		{
			await _settingsServiceProvider.AddSettingsAsync(new AppSettings
			{
				CompanyName = companySettings.CompanyName,
				TaxId = companySettings.TaxId,
				Currency = companySettings.Currency,
				VatRatePercent = companySettings.VatRatePercent,
				PaymentTermInDays = companySettings.PaymentTermInDays,
				FirstInvoiceNumber = companySettings.FirstInvoiceNumber,
				FirstCustomerId = companySettings.FirstCustomerId,
				CurrencyDecimalDigits = companySettings.CurrencyDecimalDigits
			});
		}
	}
}
