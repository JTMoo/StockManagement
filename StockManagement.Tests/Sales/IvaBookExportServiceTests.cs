using Moq;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Sales.Core;
using StockManagement.Settings.Core.Contracts;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class IvaBookExportServiceTests
{
	private readonly Mock<IInvoiceServiceProvider> _invoices = new();
	private readonly Mock<ICreditNoteServiceProvider> _creditNotes = new();
	private readonly Mock<ISettingsService> _settings = new();

	private readonly DateTime _from = new(2026, 9, 1);
	private readonly DateTime _to = new(2026, 9, 30);


	[TestInitialize]
	public void Initialize()
	{
		_settings.Setup(service => service.GetCompanySettingsAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CompanySettings("Acme", "1", "PYG", 10, 30, 1, 1001, 0));
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
	}

	[TestMethod]
	public async Task GetRowsAsync_Invoice_MapsNumberDateCustomerAndRatesFromItems()
	{
		// Arrange
		var customer = new Customer() { IdentificationNumber = "80012345-6", Name = "Jane", Lastname = "Doe", CustomerId = 1001 };
		var invoice = new Invoice()
		{
			Number = "001-001-0000001",
			Date = new DateTime(2026, 9, 15),
			Total = 110,
			Customer = customer,
			Items = [new ShoppingCartItem(new StockItem("Soda", amount: 5, price: 110) { VatRatePercent = 10 }) { Amount = 1 }]
		};
		this.SetupInvoicePage(invoice);

		// Act
		var result = await this.CreateService().GetRowsAsync(_from, _to);

		// Assert
		Assert.AreEqual(1, result.Count);
		var row = result[0];
		Assert.AreEqual("1", row.DocumentTypeCode);
		Assert.AreEqual("001-001-0000001", row.Number);
		Assert.AreEqual(invoice.Date, row.Date);
		Assert.AreEqual("80012345-6", row.CustomerRuc);
		Assert.AreEqual(customer.Display, row.CustomerName);
		Assert.AreEqual(100, row.Taxed10);
		Assert.AreEqual(10, row.Vat10);
		Assert.AreEqual(110, row.Total);
	}

	[TestMethod]
	public async Task GetRowsAsync_CreditNote_UsesCancelledInvoicesBreakdownAndOwnNumberAndDate()
	{
		// Arrange
		var customer = new Customer() { IdentificationNumber = "80012345-6" };
		var invoice = new Invoice()
		{
			Number = "001-001-0000002",
			Customer = customer,
			Items = [new ShoppingCartItem(new StockItem("Milk", amount: 5, price: 105) { VatRatePercent = 5 }) { Amount = 1 }]
		};
		var creditNote = new CreditNote() { Number = 4, Date = new DateTime(2026, 9, 20), Total = 105, Invoice = invoice };
		this.SetupInvoicePage();
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([creditNote]);

		// Act
		var result = await this.CreateService().GetRowsAsync(_from, _to);

		// Assert
		Assert.AreEqual(1, result.Count);
		var row = result[0];
		Assert.AreEqual("5", row.DocumentTypeCode);
		Assert.AreEqual("4", row.Number);
		Assert.AreEqual(creditNote.Date, row.Date);
		Assert.AreEqual("80012345-6", row.CustomerRuc);
		Assert.AreEqual(100, row.Taxed5);
		Assert.AreEqual(5, row.Vat5);
	}

	[TestMethod]
	public async Task GetRowsAsync_CreditNoteOutsideRange_IsExcluded()
	{
		// Arrange
		var creditNote = new CreditNote() { Number = 1, Date = _to.AddDays(1), Invoice = new Invoice() };
		this.SetupInvoicePage();
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([creditNote]);

		// Act
		var result = await this.CreateService().GetRowsAsync(_from, _to);

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public async Task GetRowsAsync_PagedInvoices_FollowsCursorUntilLastPage()
	{
		// Arrange
		var first = new Invoice() { Number = "1", Date = new DateTime(2026, 9, 5), Customer = new Customer(), Items = [] };
		var second = new Invoice() { Number = "2", Date = new DateTime(2026, 9, 6), Customer = new Customer(), Items = [] };
		_invoices.Setup(provider => provider.GetInvoicesAsync(null, _from, _to, null, 100, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CursorPage<Invoice>([first], "cursor-2"));
		_invoices.Setup(provider => provider.GetInvoicesAsync(null, _from, _to, "cursor-2", 100, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CursorPage<Invoice>([second], null));

		// Act
		var result = await this.CreateService().GetRowsAsync(_from, _to);

		// Assert
		Assert.AreEqual(2, result.Count);
		CollectionAssert.AreEquivalent(new[] { "1", "2" }, result.Select(row => row.Number).ToList());
	}

	private void SetupInvoicePage(params Invoice[] invoices)
	{
		_invoices.Setup(provider => provider.GetInvoicesAsync(null, _from, _to, null, 100, It.IsAny<CancellationToken>()))
			.ReturnsAsync(new CursorPage<Invoice>(invoices, null));
	}

	private IvaBookExportService CreateService()
	{
		return new IvaBookExportService(_invoices.Object, _creditNotes.Object, _settings.Object);
	}
}
