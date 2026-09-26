using Moq;
using StockManagement.Import.Core;
using StockManagement.Import.Core.Contracts;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class StockItemImportTargetHandlerTests
{
	private readonly Mock<IStockItemImportService> _importService = new();
	private readonly Mock<IStockItemServiceProvider> _stockItems = new();
	private readonly StockItemImportTargetHandler _handler;


	public StockItemImportTargetHandlerTests()
	{
		_handler = new StockItemImportTargetHandler(_importService.Object, _stockItems.Object);
	}


	[TestMethod]
	public void Target_IsStockItems()
	{
		Assert.AreEqual(ImportTarget.StockItems, _handler.Target);
	}

	[TestMethod]
	public async Task SplitDuplicatesAsync_DelegatesToImportService()
	{
		// Arrange
		object fresh = new StockItem("Fresh", code: "B2");
		object clash = new StockItem("Clash", code: "A1");
		_importService.Setup(service => service.SplitDuplicatesAsync(It.IsAny<IEnumerable<StockItem>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new DuplicateFilterResult<StockItem>([(StockItem)fresh], [(StockItem)clash]));

		// Act
		var result = await _handler.SplitDuplicatesAsync([fresh, clash]);

		// Assert
		CollectionAssert.AreEqual(new[] { fresh }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { clash }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task CommitAsync_Candidates_ImportsThemAndReturnsTheirIds()
	{
		// Arrange
		object item = new StockItem("Fresh", code: "B2") { Id = "generated-id" };

		// Act
		var ids = await _handler.CommitAsync([item]);

		// Assert
		_importService.Verify(service => service.ImportAsync(It.Is<IEnumerable<StockItem>>(list => list.SequenceEqual(new[] { item })), It.IsAny<CancellationToken>()), Times.Once);
		CollectionAssert.AreEqual(new[] { "generated-id" }, ids.ToList());
	}

	[TestMethod]
	public async Task UndoAsync_KnownId_DeletesTheStockItem()
	{
		// Arrange
		var stockItem = new StockItem("Fresh", code: "B2");
		_stockItems.Setup(provider => provider.GetStockItemByIdAsync("id-1")).ReturnsAsync(stockItem);

		// Act
		await _handler.UndoAsync([("id-1", stockItem)]);

		// Assert
		_stockItems.Verify(provider => provider.DeleteStockItemAsync(stockItem), Times.Once);
	}

	[TestMethod]
	public void SerializeThenDeserializeCandidate_RoundTripsFields()
	{
		// Arrange
		var stockItem = new StockItem("Screw", code: "A1", amount: 5);

		// Act
		var restored = (StockItem)_handler.DeserializeCandidate(_handler.SerializeCandidate(stockItem));

		// Assert
		Assert.AreEqual("Screw", restored.Name);
		Assert.AreEqual("A1", restored.Code);
		Assert.AreEqual(5, restored.Amount);
	}
}
