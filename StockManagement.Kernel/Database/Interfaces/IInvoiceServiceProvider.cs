using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database.Interfaces;


public interface IInvoiceServiceProvider
{
	public Task<Invoice> GetInvoiceAync(int invoiceNumber);
	public Task<IEnumerable<Invoice>> GetInvoicesAsync();

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> UpdateInvoiceAsync(Invoice invoice);

	/// <returns>Rows affected; 1 on success</returns>
	public Task<int> DeleteInvoiceAsync(Invoice invoice);

	/// <exception cref="InvoiceNumberAlreadyExistsException">Number already in use; nothing written</exception>
	public Task AddInvoiceAsync(Invoice invoice);

	/// <summary>
	/// Takes every line of <paramref name="invoice"/> out of stock and stores the invoice, all in one transaction
	/// </summary>
	/// <remarks>
	/// A line counts as short when its article is unknown or has less than the line's amount; then nothing is written.
	/// Records one <see cref="Transaction"/> per line. Sets each line's <see cref="StockItem.Amount"/> to the stock left.
	/// </remarks>
	/// <returns>Names of the short lines; empty when the sale was stored</returns>
	/// <exception cref="InvoiceNumberAlreadyExistsException">Invoice number already exists; nothing written</exception>
	public Task<IReadOnlyList<string>> TryAddSaleAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
