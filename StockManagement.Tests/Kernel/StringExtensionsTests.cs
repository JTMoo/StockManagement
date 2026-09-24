using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class StringExtensionsTests
{
	[TestMethod]
	[DataRow("Filter", null)]
	[DataRow("Filter", "")]
	[DataRow(null, "")]
	public void MatchesSearch_EmptySearch_ReturnsTrue(string value, string search)
	{
		// Act
		var result = value.MatchesSearch(search);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow("Oil filter", "FILTER")]
	[DataRow("Oil filter", "l f")]
	[DataRow("A(1)", "(")]
	[DataRow("A.1", ".")]
	public void MatchesSearch_ValueContainsSearch_ReturnsTrue(string value, string search)
	{
		// Act
		var result = value.MatchesSearch(search);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow("Oil filter", "air")]
	[DataRow("A1", ".")]
	[DataRow(null, "A")]
	public void MatchesSearch_ValueLacksSearch_ReturnsFalse(string value, string search)
	{
		// Act
		var result = value.MatchesSearch(search);

		// Assert
		Assert.IsFalse(result);
	}
}
