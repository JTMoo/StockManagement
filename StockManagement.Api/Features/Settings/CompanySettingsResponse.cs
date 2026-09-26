using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Api.Features.Settings;


public sealed record CompanySettingsResponse(string CompanyName, string TaxId, string Currency, decimal VatRatePercent, int PaymentTermInDays, int FirstInvoiceNumber, int FirstCustomerId)
{
	public static CompanySettingsResponse From(CompanySettings settings)
	{
		return new CompanySettingsResponse(settings.CompanyName, settings.TaxId, settings.Currency, settings.VatRatePercent, settings.PaymentTermInDays, settings.FirstInvoiceNumber, settings.FirstCustomerId);
	}
}
