using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public interface ISaleService
{
	/// <summary>
	/// Gross total of the cart
	/// </summary>
	/// <remarks>Each unit price is rounded to a whole unit, then multiplied by the quantity.</remarks>
	public long CalculateTotal(IEnumerable<ShoppingCartItem> items);

	/// <summary>
	/// Builds an unsaved invoice for the cart with total, tax and due date filled in
	/// </summary>
	/// <remarks>The number is left at 0; see <see cref="GetNextInvoiceNumberAsync"/>.</remarks>
	public Invoice CreateInvoice(Customer customer, IEnumerable<ShoppingCartItem> items, DateTime date);

	/// <summary>
	/// Number for the next new invoice
	/// </summary>
	/// <remarks>Highest stored number + 1, or 1 when no invoice exists.</remarks>
	public Task<int> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Takes the sold units out of stock and stores the invoice
	/// </summary>
	/// <remarks>Nothing is written when any article has too little stock.</remarks>
	public Task<SaleResult> CompleteSaleAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
