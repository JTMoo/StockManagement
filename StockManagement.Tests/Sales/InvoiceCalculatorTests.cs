using StockManagement.Sales.Core;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class InvoiceCalculatorTests
{
	[TestMethod]
	public void CalculateTotal_NoLines_ReturnsZero()
	{
		// Act
		var result = InvoiceCalculator.CalculateTotal([], 0);

		// Assert
		Assert.AreEqual(0, result);
	}

	[TestMethod]
	public void CalculateTotal_NullLines_ReturnsZero()
	{
		// Act
		var result = InvoiceCalculator.CalculateTotal(null, 0);

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
		var result = InvoiceCalculator.CalculateTotal(lines, 0);

		// Assert
		Assert.AreEqual(5000, result);
	}

	[DataTestMethod]
	[DataRow(99.4, 99)]
	[DataRow(99.6, 100)]
	[DataRow(2.5, 3)]
	[DataRow(3.5, 4)]
	public void CalculateTotal_ZeroDigits_RoundsUnitPriceAwayFromZero(double unitPrice, long expected)
	{
		// Arrange
		List<SaleLine> lines = [new("A1", "Screw", 1, (decimal)unitPrice)];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines, 0);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void CalculateTotal_ZeroDigits_RoundsBeforeMultiplying()
	{
		// Arrange
		List<SaleLine> lines = [new("A1", "Screw", 10, 0.4m)];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines, 0);

		// Assert
		Assert.AreEqual(0, result);
	}

	[TestMethod]
	public void CalculateTotal_TwoDigits_RoundsUnitPriceToCents()
	{
		// Arrange
		List<SaleLine> lines = [new("A1", "Screw", 2, 1.005m)];

		// Act
		var result = InvoiceCalculator.CalculateTotal(lines, 2);

		// Assert
		Assert.AreEqual(2.02m, result);
	}

	[DataTestMethod]
	[DataRow(0d, 10d, 0d)]
	[DataRow(110d, 10d, 10d)]
	[DataRow(11000d, 10d, 1000d)]
	[DataRow(100d, 10d, 9d)]
	[DataRow(105d, 10d, 10d)]
	public void CalculateTax_ZeroDigits_ReturnsVatShareRoundedToWholeUnit(double total, double vatRatePercent, double expected)
	{
		// Act
		var result = InvoiceCalculator.CalculateTax((decimal)total, (decimal)vatRatePercent, 0);

		// Assert
		Assert.AreEqual((decimal)expected, result);
	}

	[DataTestMethod]
	[DataRow(0d, 5d, 0d)]
	[DataRow(105d, 5d, 5d)]
	[DataRow(2100d, 5d, 100d)]
	public void CalculateTax_FivePercentRate_ReturnsVatShare(double total, double vatRatePercent, double expected)
	{
		// Act
		var result = InvoiceCalculator.CalculateTax((decimal)total, (decimal)vatRatePercent, 0);

		// Assert
		Assert.AreEqual((decimal)expected, result);
	}

	[TestMethod]
	public void CalculateTax_TwoDigits_ReturnsVatShareRoundedToCents()
	{
		// Act
		var result = InvoiceCalculator.CalculateTax(11m, 10m, 2);

		// Assert
		Assert.AreEqual(1.00m, result);
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
