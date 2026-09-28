using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;
using StockManagement.Sales.Core;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class InvoiceStatusCalculatorTests
{
	private static readonly DateTime Now = new(2026, 9, 27);

	[TestMethod]
	public void GetStatus_NoPayments_ReturnsOpen()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(10));

		// Act
		var status = InvoiceStatusCalculator.GetStatus(invoice, Now);

		// Assert
		Assert.AreEqual(InvoiceStatus.Open, status);
	}

	[TestMethod]
	public void GetStatus_PartialPayment_ReturnsPartiallyPaid()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(10), new Payment { Amount = 400 });

		// Act
		var status = InvoiceStatusCalculator.GetStatus(invoice, Now);

		// Assert
		Assert.AreEqual(InvoiceStatus.PartiallyPaid, status);
	}

	[TestMethod]
	public void GetStatus_PaymentsCoverTotal_ReturnsPaid()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(-10), new Payment { Amount = 600 }, new Payment { Amount = 400 });

		// Act
		var status = InvoiceStatusCalculator.GetStatus(invoice, Now);

		// Assert
		Assert.AreEqual(InvoiceStatus.Paid, status);
	}

	[TestMethod]
	public void GetStatus_UnpaidPastExpiration_ReturnsOverdue()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(-1));

		// Act
		var status = InvoiceStatusCalculator.GetStatus(invoice, Now);

		// Assert
		Assert.AreEqual(InvoiceStatus.Overdue, status);
	}

	[TestMethod]
	public void GetStatus_PartiallyPaidPastExpiration_ReturnsOverdue()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(-1), new Payment { Amount = 500 });

		// Act
		var status = InvoiceStatusCalculator.GetStatus(invoice, Now);

		// Assert
		Assert.AreEqual(InvoiceStatus.Overdue, status);
	}

	[TestMethod]
	public void AmountDue_SubtractsPaymentsFromTotal()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(10), new Payment { Amount = 300 });

		// Act
		var amountDue = InvoiceStatusCalculator.AmountDue(invoice);

		// Assert
		Assert.AreEqual(700, amountDue);
	}

	[TestMethod]
	public void AmountPaid_NoPayments_ReturnsZero()
	{
		// Arrange
		var invoice = CreateInvoice(total: 1000, expirationDate: Now.AddDays(10));

		// Act
		var amountPaid = InvoiceStatusCalculator.AmountPaid(invoice);

		// Assert
		Assert.AreEqual(0, amountPaid);
	}

	private static Invoice CreateInvoice(decimal total, DateTime expirationDate, params Payment[] payments)
	{
		return new Invoice { Total = total, ExpirationDate = expirationDate, Payments = [.. payments] };
	}
}
