using StockManagement.Kernel.Util;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class SequenceNumberTests
{
	[TestMethod]
	public void Next_NoNumbersInUse_ReturnsFirst()
	{
		// Act
		var result = SequenceNumber.Next([], 1001);

		// Assert
		Assert.AreEqual(1001, result);
	}

	[TestMethod]
	public void Next_NullNumbers_ReturnsFirst()
	{
		// Act
		var result = SequenceNumber.Next(null, 1);

		// Assert
		Assert.AreEqual(1, result);
	}

	[TestMethod]
	public void Next_UnorderedNumbersWithGaps_ReturnsHighestPlusOne()
	{
		// Act
		var result = SequenceNumber.Next([4, 12, 7], 1);

		// Assert
		Assert.AreEqual(13, result);
	}

	[TestMethod]
	public void Next_HighestBelowFirst_ReturnsHighestPlusOne()
	{
		// Act
		var result = SequenceNumber.Next([5], 1001);

		// Assert
		Assert.AreEqual(6, result);
	}
}
