using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Util;
using StockManagement.Sales.Core.Contracts;

namespace StockManagement.Sales.Core;


internal class SaleService(IStockItemServiceProvider stockItemServiceProvider, IInvoiceServiceProvider invoiceServiceProvider) : ISaleService
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;
	private readonly IInvoiceServiceProvider _invoiceServiceProvider = invoiceServiceProvider;


	public long CalculateTotal(IEnumerable<ShoppingCartItem> items)
	{
		return InvoiceCalculator.CalculateTotal((items ?? []).Select(ToSaleLine));
	}

	public Invoice CreateInvoice(Customer customer, IEnumerable<ShoppingCartItem> items, DateTime date)
	{
		List<ShoppingCartItem> cartItems = [.. items ?? []];
		var total = this.CalculateTotal(cartItems);

		return new Invoice()
		{
			Customer = customer,
			Date = date,
			ExpirationDate = InvoiceCalculator.CalculateExpirationDate(date),
			Items = cartItems,
			Total = total,
			Tax = InvoiceCalculator.CalculateTax(total)
		};
	}

	/// <remarks>Still "highest + 1" over all stored invoices; a counter document with $inc is the planned replacement.</remarks>
	public async Task<int> GetNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var invoices = await _invoiceServiceProvider.GetInvoicesAsync() ?? [];
		return SequenceNumber.Next(invoices.Select(invoice => invoice.Number), InvoiceCalculator.FirstInvoiceNumber);
	}

	/// <remarks>
	/// Stock is checked against the database, not against the possibly stale articles held by the cart.
	/// The writes are still one by one (no Mongo transaction yet), but a shortage no longer leaves stock half updated.
	/// </remarks>
	public async Task<SaleResult> CompleteSaleAsync(Invoice invoice, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(invoice);

		var items = invoice.Items ?? [];
		var currentStock = await this.LoadCurrentStockAsync(items, cancellationToken);

		var requests = items.Select(item => new StockRequest(item.StockItem.Code, item.StockItem.Name, item.Amount, currentStock.GetValueOrDefault(item.StockItem.Code)?.Amount ?? 0));
		var shortages = StockAvailability.FindShortages(requests);
		if (shortages.Count > 0) return new SaleResult(shortages.Select(shortage => shortage.Name).ToList());

		foreach (var item in items)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var stockItem = currentStock[item.StockItem.Code];
			stockItem.Amount -= item.Amount;
			item.StockItem.Amount = stockItem.Amount;
			await _stockItemServiceProvider.UpdateStockItemAsync(stockItem);
		}

		await _invoiceServiceProvider.AddInvoiceAsync(invoice);
		return SaleResult.Success;
	}

	private async Task<Dictionary<string, StockItem>> LoadCurrentStockAsync(IEnumerable<ShoppingCartItem> items, CancellationToken cancellationToken)
	{
		Dictionary<string, StockItem> currentStock = [];
		foreach (var code in items.Select(item => item.StockItem.Code).Distinct())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (await _stockItemServiceProvider.GetStockItemAsync(code) is not StockItem stockItem) continue;
			currentStock[code] = stockItem;
		}

		return currentStock;
	}

	private static SaleLine ToSaleLine(ShoppingCartItem item)
	{
		return new SaleLine(item.StockItem.Code, item.StockItem.Name, item.Amount, item.StockItem.Price);
	}
}
