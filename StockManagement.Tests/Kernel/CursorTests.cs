using StockManagement.Kernel.Database;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class CursorTests
{
	[TestMethod]
	public void EncodeThenTryDecode_RoundTrips()
	{
		// Act
		var cursor = Cursor.Encode("2026-01-01", "abc123");
		var parts = Cursor.TryDecode(cursor, 2);

		// Assert
		CollectionAssert.AreEqual(new[] { "2026-01-01", "abc123" }, parts);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	public void TryDecode_MissingCursor_ReturnsNull(string? cursor)
	{
		// Act
		var parts = Cursor.TryDecode(cursor, 2);

		// Assert
		Assert.IsNull(parts);
	}

	[TestMethod]
	public void TryDecode_NotBase64_ReturnsNull()
	{
		// Act
		var parts = Cursor.TryDecode("not valid base64!!", 2);

		// Assert
		Assert.IsNull(parts);
	}

	[TestMethod]
	public void TryDecode_WrongPartCount_ReturnsNull()
	{
		// Arrange
		var cursor = Cursor.Encode("onlyOnePart");

		// Act
		var parts = Cursor.TryDecode(cursor, 2);

		// Assert
		Assert.IsNull(parts);
	}
}
