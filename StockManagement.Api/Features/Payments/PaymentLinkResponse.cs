using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Payments;


public sealed record PaymentLinkResponse(string ExternalId, string QrUrl, decimal Amount, PaymentLinkStatus Status, DateTime CreatedAt, DateTime ExpiresAt)
{
	public static PaymentLinkResponse From(PaymentLink link)
	{
		return new(link.ExternalId, link.QrUrl, link.Amount, link.Status, link.CreatedAt, link.ExpiresAt);
	}
}
