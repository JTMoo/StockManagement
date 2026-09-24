using FastEndpoints;
using StockManagement.Kernel.Model.Types;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public sealed record UpdateSettingsRequest(AvailableLanguages Language);


public class UpdateSettingsEndpoint(ISettingsService settingsService) : Endpoint<UpdateSettingsRequest, SettingsResponse>
{
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Put("/settings");
	}

	public override async Task<SettingsResponse> ExecuteAsync(UpdateSettingsRequest request, CancellationToken cancellationToken)
	{
		await _settingsService.SetLanguageAsync(request.Language, cancellationToken);
		return new SettingsResponse(request.Language);
	}
}
