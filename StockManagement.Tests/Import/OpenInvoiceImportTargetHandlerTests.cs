using Moq;
using StockManagement.Import.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class OpenInvoiceImportTargetHandlerTests
{
	private readonly Mock<ICustomerServiceProvider> _customers = new();
	private readonly Mock<IInvoiceServiceProvider> _invoices = new();
	private readonly OpenInvoiceImportTargetHandler _handler;


	public OpenInvoiceImportTargetHandlerTests()
	{
		_handler = new OpenInvoiceImportTargetHandler(_customers.Object, _invoices.Object);
	}


	[TestMethod]
	public void Target_IsOpenInvoices()
	{
		Assert.AreEqual(ImportTarget.OpenInvoices, _handler.Target);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_NewNumberAndKnownCustomer_IsUnique()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([]);
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new Customer { IdentificationNumber = "123" }]);
		object row = MakeRow("123", 1);

		// Act
		var result = await _handler.SplitDuplicatesAsync([row]);

		// Assert
		CollectionAssert.AreEqual(new[] { row }, result.Unique.ToList());
		Assert.AreEqual(0, result.Duplicates.Count);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_NumberAlreadyStored_IsDuplicate()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([new Invoice { Number = "1" }]);
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new Customer { IdentificationNumber = "123" }]);
		object row = MakeRow("123", 1);

		// Act
		var result = await _handler.SplitDuplicatesAsync([row]);

		// Assert
		Assert.AreEqual(0, result.Unique.Count);
		CollectionAssert.AreEqual(new[] { row }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_NumberRepeatedInFile_KeepsFirstAsDuplicate()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([]);
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([new Customer { IdentificationNumber = "123" }]);
		object first = MakeRow("123", 1);
		object second = MakeRow("123", 1);

		// Act
		var result = await _handler.SplitDuplicatesAsync([first, second]);

		// Assert
		CollectionAssert.AreEqual(new[] { first }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { second }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_NoMatchingCustomer_IsDuplicate()
	{
		// Arrange
		_invoices.Setup(provider => provider.GetInvoicesAsync()).ReturnsAsync([]);
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
		object row = MakeRow("Unknown", 1);

		// Act
		var result = await _handler.SplitDuplicatesAsync([row]);

		// Assert
		Assert.AreEqual(0, result.Unique.Count);
		CollectionAssert.AreEqual(new[] { row }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task CommitAsync_AmountPaidZero_StoresInvoiceWithNoPayments()
	{
		// Arrange
		var customer = new Customer { IdentificationNumber = "123" };
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([customer]);
		Invoice? stored = null;
		_invoices.Setup(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()))
			.Callback<Invoice>(invoice => { invoice.Id = "invoice-1"; stored = invoice; })
			.Returns(Task.CompletedTask);
		var row = MakeRow("123", 1);
		row.Total = 1000;
		row.AmountPaid = 0;

		// Act
		var ids = await _handler.CommitAsync([row]);

		// Assert
		CollectionAssert.AreEqual(new[] { "invoice-1" }, ids.ToList());
		Assert.AreSame(customer, stored!.Customer);
		Assert.AreEqual(SaleCondition.Credit, stored.SaleCondition);
		Assert.AreEqual(0, stored.Payments.Count);
	}

	[TestMethod]
	public async Task CommitAsync_AmountPaidPositive_SeedsOnePayment()
	{
		// Arrange
		var customer = new Customer { IdentificationNumber = "123" };
		_customers.Setup(provider => provider.GetCustomersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([customer]);
		Invoice? stored = null;
		_invoices.Setup(provider => provider.AddInvoiceAsync(It.IsAny<Invoice>()))
			.Callback<Invoice>(invoice => { invoice.Id = "invoice-1"; stored = invoice; })
			.Returns(Task.CompletedTask);
		var row = MakeRow("123", 1);
		row.Total = 1000;
		row.AmountPaid = 400;

		// Act
		await _handler.CommitAsync([row]);

		// Assert
		var payment = stored!.Payments.Single();
		Assert.AreEqual(400, payment.Amount);
		Assert.AreEqual(PaymentMethod.Other, payment.Method);
	}

	[TestMethod]
	public async Task UndoAsync_KnownNumber_DeletesTheInvoice()
	{
		// Arrange
		var invoice = new Invoice { Number = "1" };
		_invoices.Setup(provider => provider.GetInvoiceAync("1")).ReturnsAsync(invoice);
		var row = MakeRow("123", 1);

		// Act
		await _handler.UndoAsync([("invoice-1", row)]);

		// Assert
		_invoices.Verify(provider => provider.DeleteInvoiceAsync(invoice), Times.Once);
	}

	[TestMethod]
	public void SerializeThenDeserializeCandidate_RoundTripsFields()
	{
		// Arrange
		var row = MakeRow("123", 1);
		row.Total = 1000;
		row.Tax = 100;
		row.AmountPaid = 400;

		// Act
		var restored = (OpenInvoiceRow)_handler.DeserializeCandidate(_handler.SerializeCandidate(row));

		// Assert
		Assert.AreEqual(row.CustomerIdentificationNumber, restored.CustomerIdentificationNumber);
		Assert.AreEqual(row.Number, restored.Number);
		Assert.AreEqual(row.Total, restored.Total);
		Assert.AreEqual(row.Tax, restored.Tax);
		Assert.AreEqual(row.AmountPaid, restored.AmountPaid);
	}

	private static OpenInvoiceRow MakeRow(string customerIdentificationNumber, int number) => new() { CustomerIdentificationNumber = customerIdentificationNumber, Number = number };
}
