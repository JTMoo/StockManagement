using FastEndpoints;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public class GetSettingsEndpoint(ISettingsService settingsService) : EndpointWithoutRequest<SettingsResponse>
{
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Get("/settings");
		this.AllowAnonymous();
	}

	public override async Task<SettingsResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		return new SettingsResponse(await _settingsService.GetLanguageAsync(cancellationToken));
	}
}
