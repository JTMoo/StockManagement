using Moq;
using StockManagement.Import.Core;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Import;


[TestClass]
public sealed class StockItemImportServiceTests
{
	private readonly Mock<IStockItemServiceProvider> _stockItems = new();


	[TestMethod]
	public async Task SplitDuplicatesAsync_CodesStoredOrRepeated_SeparatesThem()
	{
		// Arrange
		_stockItems.Setup(provider => provider.GetAllStockItemsAsync()).ReturnsAsync([new StockItem("Stored", code: "A1")]);
		var stored = new StockItem("Stored again", code: "A1");
		var fresh = new StockItem("Fresh", code: "B2");
		var repeatedFirst = new StockItem("Repeated", code: "C3");
		var repeatedSecond = new StockItem("Repeated", code: "C3");
		var service = new StockItemImportService(_stockItems.Object);

		// Act
		var result = await service.SplitDuplicatesAsync([stored, fresh, repeatedFirst, repeatedSecond]);

		// Assert
		CollectionAssert.AreEqual(new[] { fresh, repeatedFirst }, result.Unique.ToList());
		CollectionAssert.AreEqual(new[] { stored, repeatedSecond }, result.Duplicates.ToList());
	}

	[TestMethod]
	public async Task ImportAsync_Items_AddsThemInOneCall()
	{
		// Arrange
		List<StockItem> items = [new StockItem("Fresh", code: "B2")];
		var service = new StockItemImportService(_stockItems.Object);

		// Act
		await service.ImportAsync(items);

		// Assert
		_stockItems.Verify(provider => provider.AddManyStockItemsAsync(It.Is<IList<StockItem>>(added => added.SequenceEqual(items))), Times.Once);
	}

	[TestMethod]
	public async Task ImportAsync_NoItems_DoesNotCallDatabase()
	{
		// Arrange
		var service = new StockItemImportService(_stockItems.Object);

		// Act
		await service.ImportAsync([]);

		// Assert
		_stockItems.Verify(provider => provider.AddManyStockItemsAsync(It.IsAny<IList<StockItem>>()), Times.Never);
	}
}
