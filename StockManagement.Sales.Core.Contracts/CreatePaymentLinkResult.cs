using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public enum CreatePaymentLinkError
{
	InvoiceNotFound,

	/// <summary>Invoice has no amount due - nothing to collect</summary>
	AlreadyPaid,

	GatewayError
}


/// <summary>
/// Outcome of <see cref="IPaymentLinkService.CreateForInvoiceAsync"/>.
/// </summary>
public sealed record CreatePaymentLinkResult(bool Succeeded, PaymentLink? Link = null, CreatePaymentLinkError? Error = null)
{
	public static CreatePaymentLinkResult Success(PaymentLink link)
	{
		return new(true, link);
	}

	public static CreatePaymentLinkResult Failure(CreatePaymentLinkError error)
	{
		return new(false, Error: error);
	}
}
