using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IPaymentLinkServiceProvider
{
	public Task<PaymentLink?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

	/// <summary>Most recently created link for the invoice, if any</summary>
	public Task<PaymentLink?> GetLatestForInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default);

	public Task AddAsync(PaymentLink link, CancellationToken cancellationToken = default);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateAsync(PaymentLink link, CancellationToken cancellationToken = default);
}
