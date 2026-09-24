using StockManagement.Kernel.Model.Types;

namespace StockManagement.Settings.Core.Contracts;


public interface ISettingsService
{
	/// <summary>
	/// The application's UI language
	/// </summary>
	public Task<AvailableLanguages> GetLanguageAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Stores the application's UI language
	/// </summary>
	public Task SetLanguageAsync(AvailableLanguages language, CancellationToken cancellationToken = default);
}
