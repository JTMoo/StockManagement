using Moq;
using StockManagement.Import.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class OpeningStockImportTargetHandlerTests
{
	private readonly Mock<IStockItemServiceProvider> _stockItems = new();
	private readonly OpeningStockImportTargetHandler _handler;


	public OpeningStockImportTargetHandlerTests()
	{
		_handler = new OpeningStockImportTargetHandler(_stockItems.Object);
	}


	[TestMethod]
	public void Target_IsOpeningStock()
	{
		Assert.AreEqual(ImportTarget.OpeningStock, _handler.Target);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_CodeMatchesExistingItemOnce_IsUnique()
	{
		// Arrange
		_stockItems.Setup(provider => provider.GetAllStockItemsAsync()).ReturnsAsync([new StockItem("Screw", code: "A1")]);
		object row = MakeRow("A1", 5);

		// Act
		var result = await _handler.SplitDuplicatesAsync([row]);

		// Assert
		CollectionAssert.AreEqual(new[] { row }, result.Unique.ToList());
		Assert.AreEqual(0, result.Duplicates.Count);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_CodeNotAnExistingItem_IsDuplicate()
	{
		// Arrange
		_stockItems.Setup(provider => provider.GetAllStockItemsAsync()).ReturnsAsync([]);
		object row = MakeRow("Unknown", 5);

		// Act
		var result = await _handler.SplitDuplicatesAsync([row]);

		// Assert
		Assert.AreEqual(0, result.Unique.Count);
		CollectionAssert.AreEqual(new[] { row }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_CodeRepeatedInFile_KeepsFirstAsDuplicate()
	{
		// Arrange
		_stockItems.Setup(provider => provider.GetAllStockItemsAsync()).ReturnsAsync([new StockItem("Screw", code: "A1")]);
		object first = MakeRow("A1", 5);
		object second = MakeRow("A1", 3);

		// Act
		var result = await _handler.SplitDuplicatesAsync([first, second]);

		// Assert
		CollectionAssert.AreEqual(new[] { first }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { second }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task CommitAsync_Candidates_ChecksInEachAndReturnsTheStockItemId()
	{
		// Arrange
		var stockItem = new StockItem("Screw", code: "A1") { Id = "item-1" };
		_stockItems.Setup(provider => provider.GetStockItemAsync("A1")).ReturnsAsync(stockItem);
		object row = MakeRow("A1", 5);

		// Act
		var ids = await _handler.CommitAsync([row]);

		// Assert
		_stockItems.Verify(provider => provider.CheckInStockItemAsync(stockItem, 5, "Opening stock import"), Times.Once);
		CollectionAssert.AreEqual(new[] { "item-1" }, ids.ToList());
	}

	[TestMethod]
	public async Task UndoAsync_KnownId_ChecksOutTheSameAmount()
	{
		// Arrange
		var stockItem = new StockItem("Screw", code: "A1");
		_stockItems.Setup(provider => provider.GetStockItemByIdAsync("item-1")).ReturnsAsync(stockItem);
		object row = MakeRow("A1", 5);

		// Act
		await _handler.UndoAsync([("item-1", row)]);

		// Assert
		_stockItems.Verify(provider => provider.TryCheckOutStockItemAsync(stockItem, 5, "Opening stock import (undo)"), Times.Once);
	}

	[TestMethod]
	public void SerializeThenDeserializeCandidate_RoundTripsFields()
	{
		// Arrange
		var row = new OpeningStockRow { Code = "A1", Amount = 5 };

		// Act
		var restored = (OpeningStockRow)_handler.DeserializeCandidate(_handler.SerializeCandidate(row));

		// Assert
		Assert.AreEqual("A1", restored.Code);
		Assert.AreEqual(5, restored.Amount);
	}

	private static OpeningStockRow MakeRow(string code, int amount) => new() { Code = code, Amount = amount };
}
