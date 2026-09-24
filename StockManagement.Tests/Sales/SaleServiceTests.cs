using Moq;
using StockManagement.Sales.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core.Contracts;

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
		_stockItems.Verify(provider => provider.TryTakeStockAsync("A1", 3, It.IsAny<CancellationToken>()), Times.Once);
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
		_stockItems.Verify(provider => provider.TryTakeStockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_StockTakenByParallelSale_ReturnsTakenLinesAndStoresNoInvoice()
	{
		// Arrange
		var nut = new StockItem("Nut", code: "B2", amount: 10);
		var screw = new StockItem("Screw", code: "A1", amount: 5);
		this.SetupStock(nut, screw);
		var invoice = CreateInvoice(
			CreateCartItem("B2", "Nut", inStock: 10, price: 100, quantity: 4),
			CreateCartItem("A1", "Screw", inStock: 5, price: 100, quantity: 3));
		_stockItems
			.Setup(provider => provider.TryTakeStockAsync("B2", 4, It.IsAny<CancellationToken>()))
			.ReturnsAsync(() =>
			{
				screw.Amount = 1;
				nut.Amount -= 4;
				return nut;
			});

		// Act
		var result = await this.CreateService().CompleteSaleAsync(invoice);

		// Assert
		CollectionAssert.AreEqual(new[] { "Screw" }, result.UnavailableItems.ToList());
		Assert.AreEqual(10, nut.Amount);
		Assert.AreEqual(1, screw.Amount);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task CompleteSaleAsync_InvoiceWriteFails_ReturnsStockAndThrows()
	{
		// Arrange
		var stored = new StockItem("Screw", code: "A1", amount: 10);
		this.SetupStock(stored);
		var invoice = CreateInvoice(CreateCartItem("A1", "Screw", inStock: 10, price: 100, quantity: 3));
		_invoices.Setup(provider => provider.AddInvoiceAsync(invoice)).ThrowsAsync(new TimeoutException());

		// Act
		await Assert.ThrowsExceptionAsync<TimeoutException>(() => this.CreateService().CompleteSaleAsync(invoice));

		// Assert
		Assert.AreEqual(10, stored.Amount);
		_stockItems.Verify(provider => provider.ReturnStockAsync("A1", 3, CancellationToken.None), Times.Once);
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
		_stockItems.Verify(provider => provider.TryTakeStockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}


	[TestMethod]
	public async Task SellAsync_EnoughStock_StoresNumberedInvoice()
	{
		// Arrange
		var stored = new StockItem("Screw", code: "A1", amount: 10, price: 5000);
		this.SetupStock(stored);
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([new Invoice() { Number = 7 }]);
		var customer = new Customer() { CustomerId = 1001 };
		var date = new DateTime(2026, 9, 1);

		// Act
		var result = await this.CreateService().SellAsync(customer, [new SaleItem("A1", 3)], SaleCondition.Cash, date);

		// Assert
		Assert.IsTrue(result.Succeeded);
		Assert.AreEqual(8, result.Invoice.Number);
		Assert.AreEqual(15000, result.Invoice.Total);
		Assert.AreEqual(SaleCondition.Cash, result.Invoice.SaleCondition);
		Assert.AreSame(customer, result.Invoice.Customer);
		Assert.AreEqual(7, stored.Amount);
		_invoices.Verify(provider => provider.AddInvoiceAsync(result.Invoice), Times.Once);
	}

	[TestMethod]
	public async Task SellAsync_MoreThanInStock_ReportsShortageAndWritesNothing()
	{
		// Arrange
		this.SetupStock(new StockItem("Screw", code: "A1", amount: 2));

		// Act
		var result = await this.CreateService().SellAsync(new Customer(), [new SaleItem("A1", 3)], SaleCondition.Cash, DateTime.Today);

		// Assert
		Assert.IsFalse(result.Succeeded);
		Assert.IsNull(result.Invoice);
		CollectionAssert.AreEqual(new[] { "Screw" }, result.UnavailableItems.ToList());
		_stockItems.Verify(provider => provider.TryTakeStockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task SellAsync_SameCodeTwiceAboveStock_ReportsShortage()
	{
		// Arrange
		this.SetupStock(new StockItem("Screw", code: "A1", amount: 4));

		// Act
		var result = await this.CreateService().SellAsync(new Customer(), [new SaleItem("A1", 2), new SaleItem("A1", 3)], SaleCondition.Cash, DateTime.Today);

		// Assert
		CollectionAssert.AreEqual(new[] { "Screw" }, result.UnavailableItems.ToList());
	}

	[TestMethod]
	public async Task SellAsync_UnknownCode_ReportsCodeUnavailable()
	{
		// Act
		var result = await this.CreateService().SellAsync(new Customer(), [new SaleItem("X9", 1)], SaleCondition.Cash, DateTime.Today);

		// Assert
		CollectionAssert.AreEqual(new[] { "X9" }, result.UnavailableItems.ToList());
		_invoices.Verify(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()), Times.Never);
	}

	[TestMethod]
	public async Task SellAsync_ZeroAmount_Throws()
	{
		// Act + Assert
		await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => this.CreateService().SellAsync(new Customer(), [new SaleItem("A1", 0)], SaleCondition.Cash, DateTime.Today));
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
			_stockItems
				.Setup(provider => provider.TryTakeStockAsync(stockItem.Code, It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((string _, int amount, CancellationToken _) =>
				{
					if (stockItem.Amount < amount) return null;
					stockItem.Amount -= amount;
					return stockItem;
				});
			_stockItems
				.Setup(provider => provider.ReturnStockAsync(stockItem.Code, It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.Callback((string _, int amount, CancellationToken _) => stockItem.Amount += amount)
				.Returns(Task.CompletedTask);
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
