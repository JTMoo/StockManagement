using Moq;
using StockManagement.Sales.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class SaleServiceTests
{
	private readonly Mock<IStockItemServiceProvider> _stockItems = new();
	private readonly Mock<IInvoiceServiceProvider> _invoices = new();


	[TestMethod]
	public void CreateInvoice_CartWithItems_FillsTotalTaxAndDates()
	{
		// Arrange
		var service = this.CreateService();
		var customer = new Customer();
		var date = new DateTime(2026, 9, 1);
		List<ShoppingCartItem> items =
		[
			CreateCartItem("A1", "Screw", inStock: 10, price: 5000, quantity: 2),
			CreateCartItem("B2", "Nut", inStock: 10, price: 1000, quantity: 1)
		];

		// Act
		var invoice = service.CreateInvoice(customer, items, date);

		// Assert
		Assert.AreSame(customer, invoice.Customer);
		Assert.AreEqual(11000, invoice.Total);
		Assert.AreEqual(1000, invoice.Tax);
		Assert.AreEqual(date, invoice.Date);
		Assert.AreEqual(date.AddDays(30), invoice.ExpirationDate);
		Assert.AreEqual(0, invoice.Number);
		CollectionAssert.AreEqual(items, invoice.Items);
	}

	[TestMethod]
	public void CalculateTotal_CartItems_UsesRoundedUnitPrices()
	{
		// Arrange
		List<ShoppingCartItem> items =
		[
			CreateCartItem("A1", "Screw", inStock: 10, price: 5000, quantity: 2),
			new ShoppingCartItem(new StockItem("Nut", code: "B2", amount: 10) { Price = 99.6 }) { Amount = 1 }
		];

		// Act
		var result = this.CreateService().CalculateTotal(items);

		// Assert
		Assert.AreEqual(10100, result);
	}

	[TestMethod]
	public async Task GetNextInvoiceNumberAsync_NoInvoices_ReturnsOne()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([]);

		// Act
		var result = await this.CreateService().GetNextInvoiceNumberAsync();

		// Assert
		Assert.AreEqual(1, result);
	}

	[TestMethod]
	public async Task GetNextInvoiceNumberAsync_ExistingInvoices_ReturnsHighestPlusOne()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([new Invoice() { Number = 3 }, new Invoice() { Number = 41 }]);

		// Act
		var result = await this.CreateService().GetNextInvoiceNumberAsync();

		// Assert
		Assert.AreEqual(42, result);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_EnoughStock_ReducesStockAndStoresInvoice()
	{
		// Arrange
		var stored = new StockItem("Screw", code: "A1", amount: 10);
		this.SetupStock(stored);
		var invoice = CreateInvoice(CreateCartItem("A1", "Screw", inStock: 10, price: 100, quantity: 3));
		var service = this.CreateService();

		// Act
		var result = await service.CompleteSaleAsync(invoice);

		// Assert
		Assert.IsTrue(result.Succeeded);
		Assert.AreEqual(7, stored.Amount);
		Assert.AreEqual(7, invoice.Items[0].StockItem.Amount);
		_stockItems.Verify(provider => provider.UpdateStockItemAsync(stored), Times.Once);
		_invoices.Verify(provider => provider.AddInvoiceAsync(invoice), Times.Once);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_StockDroppedSinceCartWasFilled_WritesNothing()
	{
		// Arrange
		this.SetupStock(new StockItem("Screw", code: "A1", amount: 2), new StockItem("Nut", code: "B2", amount: 10));
		var invoice = CreateInvoice(
			CreateCartItem("B2", "Nut", inStock: 10, price: 100, quantity: 1),
			CreateCartItem("A1", "Screw", inStock: 10, price: 100, quantity: 3));
		var service = this.CreateService();

		// Act
		var result = await service.CompleteSaleAsync(invoice);

		// Assert
		Assert.IsFalse(result.Succeeded);
		CollectionAssert.AreEqual(new[] { "Screw" }, result.UnavailableItems.ToList());
		_stockItems.Verify(provider => provider.UpdateStockItemAsync(It.IsAny<StockItem>()), Times.Never);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_ArticleDeletedMeanwhile_ReportsItUnavailable()
	{
		// Arrange
		var invoice = CreateInvoice(CreateCartItem("A1", "Screw", inStock: 10, price: 100, quantity: 1));
		var service = this.CreateService();

		// Act
		var result = await service.CompleteSaleAsync(invoice);

		// Assert
		Assert.IsFalse(result.Succeeded);
		CollectionAssert.AreEqual(new[] { "Screw" }, result.UnavailableItems.ToList());
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_Cancelled_WritesNothing()
	{
		// Arrange
		this.SetupStock(new StockItem("Screw", code: "A1", amount: 10));
		var invoice = CreateInvoice(CreateCartItem("A1", "Screw", inStock: 10, price: 100, quantity: 1));
		var service = this.CreateService();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		// Act
		await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => service.CompleteSaleAsync(invoice, cancellation.Token));

		// Assert
		_stockItems.Verify(provider => provider.UpdateStockItemAsync(It.IsAny<StockItem>()), Times.Never);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}


	private SaleService CreateService()
	{
		return new SaleService(_stockItems.Object, _invoices.Object);
	}

	private void SetupStock(params StockItem[] stockItems)
	{
		foreach (var stockItem in stockItems)
		{
			_stockItems.Setup(provider => provider.GetStockItemAsync(stockItem.Code)).ReturnsAsync(stockItem);
		}
	}

	private static ShoppingCartItem CreateCartItem(string code, string name, int inStock, int price, int quantity)
	{
		return new ShoppingCartItem(new StockItem(name, code: code, amount: inStock, price: price)) { Amount = quantity };
	}

	private static Invoice CreateInvoice(params ShoppingCartItem[] items)
	{
		return new Invoice() { Items = [.. items] };
	}
}
