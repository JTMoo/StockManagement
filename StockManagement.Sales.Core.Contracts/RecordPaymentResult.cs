using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public enum RecordPaymentError
{
	InvoiceNotFound,

	InvalidAmount,

	ExceedsAmountDue
}


/// <summary>
/// Outcome of <see cref="IPaymentService.RecordPaymentAsync"/>.
/// </summary>
public sealed record RecordPaymentResult(bool Succeeded, Payment? Payment = null, RecordPaymentError? Error = null)
{
	public static RecordPaymentResult Success(Payment payment)
	{
		return new(true, payment);
	}

	public static RecordPaymentResult Failure(RecordPaymentError error)
	{
		return new(false, Error: error);
	}
}
