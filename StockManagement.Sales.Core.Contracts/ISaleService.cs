using StockManagement.Kernel.Model;

namespace StockManagement.Sales.Core.Contracts;


public interface ISaleService
{
	/// <summary>
	/// Gross total of the cart: each unit price rounded to a whole unit, times the quantity.
	/// </summary>
	public long CalculateTotal(IEnumerable<ShoppingCartItem> items);

	/// <summary>
	/// Builds an unsaved invoice for the cart with total, tax and due date filled in. The number is left at 0.
	/// </summary>
	public Invoice CreateInvoice(Customer customer, IEnumerable<ShoppingCartItem> items, DateTime date);

	public Task<int> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Takes the sold units out of stock and stores the invoice.
	/// Nothing is written when any article has too little stock.
	/// </summary>
	public Task<SaleResult> CompleteSaleAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
