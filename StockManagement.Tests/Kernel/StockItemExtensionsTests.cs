using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class StockItemExtensionsTests
{
	[TestMethod]
	public void GetManufacturers_NullItems_ReturnsEmpty()
	{
		// Act
		var result = ((IEnumerable<StockItem>)null).GetManufacturers();

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void GetManufacturers_MixedCaseAndEmpty_ReturnsDistinctSorted()
	{
		// Arrange
		List<StockItem> stockItems =
		[
			new("a", manufacturer: "Samasz"),
			new("b", manufacturer: ""),
			new("c", manufacturer: "hattat"),
			new("d", manufacturer: "SAMASZ"),
			new("e", manufacturer: "Hattat")
		];

		// Act
		var result = stockItems.GetManufacturers();

		// Assert
		CollectionAssert.AreEqual(new[] { "hattat", "Samasz" }, result.ToList());
	}

	[TestMethod]
	public void Manufacturer_SurroundingWhitespace_IsTrimmed()
	{
		// Act
		var stockItem = new StockItem("a", manufacturer: "  Kuhn ");

		// Assert
		Assert.AreEqual("Kuhn", stockItem.Manufacturer);
	}

	[TestMethod]
	public void CalculateSalePrice_AppliesFactorToCostPlusAdditionalCost_RoundsToCurrencyDigits()
	{
		// Act
		var result = StockItemExtensions.CalculateSalePrice(purchasePrice: 10m, purchaseExchangeRate: 7300m, additionalPurchaseCost: 5000m, factor: 1.25m, currencyDecimalDigits: 0);

		// Assert
		Assert.AreEqual(97500m, result);
	}

	[TestMethod]
	public void CalculateSalePrice_ZeroFactor_ReturnsZero()
	{
		// Act
		var result = StockItemExtensions.CalculateSalePrice(purchasePrice: 10m, purchaseExchangeRate: 7300m, additionalPurchaseCost: 5000m, factor: 0m, currencyDecimalDigits: 0);

		// Assert
		Assert.AreEqual(0m, result);
	}

	[TestMethod]
	public void CalculateSalePrice_MidpointRoundsAwayFromZero()
	{
		// Act
		var result = StockItemExtensions.CalculateSalePrice(purchasePrice: 1m, purchaseExchangeRate: 1m, additionalPurchaseCost: 0.005m, factor: 1m, currencyDecimalDigits: 2);

		// Assert
		Assert.AreEqual(1.01m, result);
	}
}
