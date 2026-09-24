using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
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
	/// Each line is taken out of stock atomically and only if enough is left, so parallel sales cannot oversell.
	/// A failed line or invoice write returns the lines already taken. Not one Mongo transaction yet (#41).
	/// </remarks>
	public async Task<SaleResult> CompleteSaleAsync(Invoice invoice, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(invoice);

		var items = invoice.Items ?? [];
		var currentStock = await this.LoadCurrentStockAsync(items.Select(item => item.StockItem.Code), cancellationToken);

		var requests = items.Select(item => new StockRequest(item.StockItem.Code, item.StockItem.Name, item.Amount, currentStock.GetValueOrDefault(item.StockItem.Code)?.Amount ?? 0));
		var shortages = StockAvailability.FindShortages(requests);
		if (shortages.Count > 0) return new SaleResult(shortages.Select(shortage => shortage.Name).ToList());

		List<ShoppingCartItem> taken = [];
		try
		{
			foreach (var item in items)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (await _stockItemServiceProvider.TryTakeStockAsync(item.StockItem.Code, item.Amount, cancellationToken) is not StockItem stockItem)
				{
					await this.ReturnStockAsync(taken);
					return new SaleResult([item.StockItem.Name]);
				}

				item.StockItem.Amount = stockItem.Amount;
				taken.Add(item);
			}

			await _invoiceServiceProvider.AddInvoiceAsync(invoice);
		}
		catch
		{
			await this.ReturnStockAsync(taken);
			throw;
		}

		return SaleResult.Success;
	}

	/// <remarks>Stock is checked before the cart is built, because <see cref="ShoppingCartItem.Amount"/> silently caps at the units in stock.</remarks>
	public async Task<SaleResult> SellAsync(Customer customer, IReadOnlyList<SaleItem> items, SaleCondition saleCondition, DateTime date, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(customer);
		ArgumentNullException.ThrowIfNull(items);
		if (items.Any(item => item.Amount <= 0)) throw new ArgumentOutOfRangeException(nameof(items), "Every amount must be greater than 0.");

		var currentStock = await this.LoadCurrentStockAsync(items.Select(item => item.Code), cancellationToken);

		var requests = items.Select(item => new StockRequest(item.Code, currentStock.GetValueOrDefault(item.Code)?.Name ?? item.Code, item.Amount, currentStock.GetValueOrDefault(item.Code)?.Amount ?? 0));
		var shortages = StockAvailability.FindShortages(requests);
		if (shortages.Count > 0) return new SaleResult(shortages.Select(shortage => shortage.Name).ToList());

		var cartItems = items.Select(item => new ShoppingCartItem(currentStock[item.Code]) { Amount = item.Amount });
		var invoice = this.CreateInvoice(customer, cartItems, date);
		invoice.SaleCondition = saleCondition;
		invoice.Number = await this.GetNextInvoiceNumberAsync(cancellationToken);

		var result = await this.CompleteSaleAsync(invoice, cancellationToken);
		return result.Succeeded ? result with { Invoice = invoice } : result;
	}

	/// <remarks>Empties <paramref name="taken"/> one by one, so a second call never returns a line twice.</remarks>
	private async Task ReturnStockAsync(List<ShoppingCartItem> taken)
	{
		while (taken.Count > 0)
		{
			var item = taken[^1];
			await _stockItemServiceProvider.ReturnStockAsync(item.StockItem.Code, item.Amount, CancellationToken.None);
			taken.RemoveAt(taken.Count - 1);
		}
	}

	private async Task<Dictionary<string, StockItem>> LoadCurrentStockAsync(IEnumerable<string> codes, CancellationToken cancellationToken)
	{
		Dictionary<string, StockItem> currentStock = [];
		foreach (var code in codes.Distinct())
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
