using FastEndpoints;
using FluentValidation;
using StockManagement.Auth.Core.Contracts;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public sealed record UpdateCompanySettingsRequest(string CompanyName, string TaxId, string Currency, decimal VatRatePercent, int PaymentTermInDays, int FirstInvoiceNumber, int FirstCustomerId, int CurrencyDecimalDigits);


public class UpdateCompanySettingsValidator : Validator<UpdateCompanySettingsRequest>
{
	public UpdateCompanySettingsValidator()
	{
		this.RuleFor(request => request.VatRatePercent).InclusiveBetween(0, 100);
		this.RuleFor(request => request.PaymentTermInDays).GreaterThanOrEqualTo(0);
		this.RuleFor(request => request.FirstInvoiceNumber).GreaterThan(0);
		this.RuleFor(request => request.FirstCustomerId).GreaterThan(0);
		this.RuleFor(request => request.CurrencyDecimalDigits).InclusiveBetween(0, 4);
	}
}


public class UpdateCompanySettingsEndpoint(ISettingsService settingsService) : Endpoint<UpdateCompanySettingsRequest, CompanySettingsResponse>
{
	private readonly ISettingsService _settingsService = settingsService;


	public override void Configure()
	{
		this.Put("/company-settings");
		this.Permissions(Permission.SettingsWrite);
	}

	public override async Task<CompanySettingsResponse> ExecuteAsync(UpdateCompanySettingsRequest request, CancellationToken cancellationToken)
	{
		var settings = new CompanySettings(request.CompanyName, request.TaxId, request.Currency, request.VatRatePercent, request.PaymentTermInDays, request.FirstInvoiceNumber, request.FirstCustomerId, request.CurrencyDecimalDigits);
		await _settingsService.SetCompanySettingsAsync(settings, cancellationToken);
		return CompanySettingsResponse.From(settings);
	}
}
