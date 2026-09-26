using FastEndpoints;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public class GetCompanySettingsEndpoint(ISettingsService settingsService) : EndpointWithoutRequest<CompanySettingsResponse>
{
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Get("/company-settings");
		this.Permissions(Permission.SettingsRead);
	}

	public override async Task<CompanySettingsResponse> ExecuteAsync(CancellationToken cancellationToken)
	{
		return CompanySettingsResponse.From(await _settingsService.GetCompanySettingsAsync(cancellationToken));
	}
}
