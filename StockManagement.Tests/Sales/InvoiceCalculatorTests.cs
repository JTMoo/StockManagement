using StockManagement.Sales.Core;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class InvoiceCalculatorTests
{
	[TestMethod]
	public void CalculateTotal_NoLines_ReturnsZero()
	{
		// Act
		var result = InvoiceCalculator.CalculateTotal([]);

		// Assert
		Assert.AreEqual(0, result);
	}

	[TestMethod]
	public void CalculateTotal_NullLines_ReturnsZero()
	{
		// Act
		var result = InvoiceCalculator.CalculateTotal(null);

		// Assert
		Assert.AreEqual(0, result);
	}

	[TestMethod]
	public void CalculateTotal_SeveralLines_SumsQuantityTimesPrice()
	{
		// Arrange
		List<SaleLine> lines =
		[
			new("A1", "Screw", 3, 1500),
			new("B2", "Nut", 2, 250)
		];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines);

		// Assert
		Assert.AreEqual(5000, result);
	}

	[DataTestMethod]
	[DataRow(99.4, 99)]
	[DataRow(99.6, 100)]
	[DataRow(2.5, 2)]
	[DataRow(3.5, 4)]
	public void CalculateTotal_FractionalPrice_RoundsUnitPriceHalfToEven(double unitPrice, long expected)
	{
		// Arrange
		List<SaleLine> lines = [new("A1", "Screw", 1, unitPrice)];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void CalculateTotal_FractionalPrice_RoundsBeforeMultiplying()
	{
		// Arrange
		List<SaleLine> lines = [new("A1", "Screw", 10, 0.4)];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines);

		// Assert
		Assert.AreEqual(0, result);
	}

	[DataTestMethod]
	[DataRow(0L, 0L)]
	[DataRow(110L, 10L)]
	[DataRow(11000L, 1000L)]
	[DataRow(100L, 9L)]
	[DataRow(105L, 10L)]
	public void CalculateTax_TenPercentRate_ReturnsVatShare(long total, long expected)
	{
		// Act
		var result = InvoiceCalculator.CalculateTax(total, 10m);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[DataTestMethod]
	[DataRow(0L, 0L)]
	[DataRow(105L, 5L)]
	[DataRow(2100L, 100L)]
	public void CalculateTax_FivePercentRate_ReturnsVatShare(long total, long expected)
	{
		// Act
		var result = InvoiceCalculator.CalculateTax(total, 5m);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void CalculateExpirationDate_AnyDate_AddsPaymentTerm()
	{
		// Arrange
		var date = new DateTime(2026, 2, 10, 14, 30, 0);

		// Act
		var result = InvoiceCalculator.CalculateExpirationDate(date, 30);

		// Assert
		Assert.AreEqual(new DateTime(2026, 3, 12, 14, 30, 0), result);
	}
}
