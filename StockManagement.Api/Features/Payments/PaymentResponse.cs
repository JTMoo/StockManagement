using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.Payments;


public sealed record PaymentResponse(DateTime Date, decimal Amount, PaymentMethod Method)
{
	public static PaymentResponse From(Payment payment)
	{
		return new(payment.Date, payment.Amount, payment.Method);
	}
}
