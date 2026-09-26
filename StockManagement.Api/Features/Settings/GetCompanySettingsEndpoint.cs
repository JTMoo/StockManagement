using FastEndpoints;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public class GetCompanySettingsEndpoint(ISettingsService settingsService) : EndpointWithoutRequest<CompanySettingsResponse>
{
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Get("/company-settings");
	}

	public override async Task<CompanySettingsResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		return CompanySettingsResponse.From(await _settingsService.GetCompanySettingsAsync(cancellationToken));
	}
}
