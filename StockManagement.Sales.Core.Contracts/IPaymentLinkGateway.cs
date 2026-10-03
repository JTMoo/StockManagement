using StockManagement.Kernel.Model.Types;

namespace StockManagement.Sales.Core.Contracts;


/// <summary>
/// The only Bancard abstraction application code depends on (#150). Never throws for a gateway-level rejection or
/// transport error - both come back as a failed <see cref="PaymentLinkGatewayResult"/>/<see cref="PaymentLinkStatus.Failed"/>;
/// only a programming error throws.
/// </summary>
public interface IPaymentLinkGateway
{
	/// <summary>
	/// Creates a Bancard QR/payment link for <paramref name="amount"/>, tagged with <paramref name="invoiceNumber"/>
	/// as Bancard's <c>shop_process_id</c>.
	/// </summary>
	Task<PaymentLinkGatewayResult> CreateAsync(string invoiceNumber, decimal amount, CancellationToken cancellationToken = default);

	/// <summary>
	/// Current status of a previously created link, re-checked against Bancard rather than trusting a webhook payload
	/// </summary>
	Task<PaymentLinkStatus> GetStatusAsync(string externalId, CancellationToken cancellationToken = default);
}
