using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public interface IPaymentLinkService
{
	/// <summary>
	/// Creates a Bancard QR/link for the invoice's current amount due and persists it as <see cref="Kernel.Model.Types.PaymentLinkStatus.Pending"/>
	/// </summary>
	public Task<CreatePaymentLinkResult> CreateForInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default);

	public Task<PaymentLink?> GetLatestForInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default);

	/// <summary>
	/// Called from the Bancard webhook (or a manual status check). Re-checks the link's status against Bancard rather
	/// than trusting the caller; when newly <see cref="Kernel.Model.Types.PaymentLinkStatus.Paid"/>, records a
	/// <see cref="Payment"/> via <see cref="IPaymentService"/> and marks the link paid, in that order.
	/// </summary>
	/// <returns><see langword="false"/> when no link matches <paramref name="externalId"/></returns>
	public Task<bool> ConfirmAsync(string externalId, CancellationToken cancellationToken = default);
}
