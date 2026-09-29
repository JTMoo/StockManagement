using Moq;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Sales.Core;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class CreditNoteServiceTests
{
	private readonly Mock<ICreditNoteServiceProvider> _creditNotes = new();


	[TestMethod]
	public async Task GetNextCreditNoteNumberAsync_NoCreditNotes_ReturnsOne()
	{
		// Arrange
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync()).ReturnsAsync([]);

		// Act
		var result = await this.CreateService().GetNextCreditNoteNumberAsync();

		// Assert
		Assert.AreEqual(1, result);
	}

	[TestMethod]
	public async Task GetNextCreditNoteNumberAsync_ExistingCreditNotes_ReturnsHighestPlusOne()
	{
		// Arrange
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync()).ReturnsAsync([new CreditNote() { Number = 3 }, new CreditNote() { Number = 7 }]);

		// Act
		var result = await this.CreateService().GetNextCreditNoteNumberAsync();

		// Assert
		Assert.AreEqual(8, result);
	}

	[TestMethod]
	public async Task CancelInvoiceAsync_NotYetCancelled_StoresNumberedCreditNoteWithInvoiceTotals()
	{
		// Arrange
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync()).ReturnsAsync([new CreditNote() { Number = 4 }]);
		_creditNotes.Setup(provider => provider.TryAddCreditNoteAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
		var invoice = new Invoice() { Number = 12, Total = 11000, Tax = 1000 };
		var date = new DateTime(2026, 9, 27);

		// Act
		var result = await this.CreateService().CancelInvoiceAsync(invoice, "Customer returned the goods", date);

		// Assert
		Assert.IsTrue(result.Succeeded);
		Assert.AreEqual(5, result.CreditNote!.Number);
		Assert.AreEqual(date, result.CreditNote.Date);
		Assert.AreEqual("Customer returned the goods", result.CreditNote.Reason);
		Assert.AreEqual(11000, result.CreditNote.Total);
		Assert.AreEqual(1000, result.CreditNote.Tax);
		Assert.AreSame(invoice, result.CreditNote.Invoice);
	}

	[TestMethod]
	public async Task CancelInvoiceAsync_AlreadyCancelled_ReportsFailureAndWritesNoCreditNote()
	{
		// Arrange
		_creditNotes.Setup(provider => provider.GetCreditNotesAsync()).ReturnsAsync([]);
		_creditNotes.Setup(provider => provider.TryAddCreditNoteAsync(It.IsAny<CreditNote>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
		var invoice = new Invoice() { Number = 12 };

		// Act
		var result = await this.CreateService().CancelInvoiceAsync(invoice, "Duplicate cancellation", DateTime.Today);

		// Assert
		Assert.IsFalse(result.Succeeded);
		Assert.IsNull(result.CreditNote);
	}

	private CreditNoteService CreateService()
	{
		return new CreditNoteService(_creditNotes.Object);
	}
}
