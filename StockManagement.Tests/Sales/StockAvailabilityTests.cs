using StockManagement.Sales.Core;

namespace StockManagement.Tests.Sales;


[TestClass]
public sealed class StockAvailabilityTests
{
	[TestMethod]
	public void FindShortages_EnoughStock_ReturnsEmpty()
	{
		// Arrange
		List<StockRequest> requests =
		[
			new("A1", "Screw", 5, 5),
			new("B2", "Nut", 1, 10)
		];

		// Act
		var result = StockAvailability.FindShortages(requests);

		// Assert
		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void FindShortages_RequestExceedsStock_ReturnsThatArticle()
	{
		// Arrange
		List<StockRequest> requests =
		[
			new("A1", "Screw", 6, 5),
			new("B2", "Nut", 1, 10)
		];

		// Act
		var result = StockAvailability.FindShortages(requests);

		// Assert
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("Screw", result[0].Name);
	}

	[TestMethod]
	public void FindShortages_SameCodeTwice_AddsRequestsUp()
	{
		// Arrange
		List<StockRequest> requests =
		[
			new("A1", "Screw", 3, 5),
			new("A1", "Screw", 3, 5)
		];

		// Act
		var result = StockAvailability.FindShortages(requests);

		// Assert
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(6, result[0].Requested);
	}

	[TestMethod]
	public void FindShortages_NullRequests_ReturnsEmpty()
	{
		// Act
		var result = StockAvailability.FindShortages(null);

		// Assert
		Assert.AreEqual(0, result.Count);
	}
}
