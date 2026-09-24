using System.Linq.Expressions;
using MongoDB.Driver;
using Moq;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class StockItemServiceProviderTests
{
	private readonly Mock<IMongoCollection<Transaction>> _transactions = new();
	private readonly Mock<IMongoCollection<StockItem>> _stockItems = new();
	private readonly Mock<IDatabase> _database = new();


	[TestInitialize]
	public void Initialize()
	{
		_database.Setup(d => d.ConnectToMongo<Transaction>()).Returns(_transactions.Object);
		_database.Setup(d => d.ConnectToMongo<StockItem>()).Returns(_stockItems.Object);
		_transactions
			.Setup(c => c.InsertOneAsync(It.IsAny<Transaction>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new TimeoutException());
	}

	[TestMethod]
	public async Task DeleteStockItemAsync_TransactionInsertFails_ThrowsAndKeepsStockItem()
	{
		// Arrange
		var provider = new StockItemServiceProvider(_database.Object);

		// Act + Assert
		await Assert.ThrowsExceptionAsync<TimeoutException>(() => provider.DeleteStockItemAsync(new StockItem { Code = "A1" }));
		_database.Verify(d => d.Delete<StockItem>(It.IsAny<BaseDocument>()), Times.Never);
	}

	[TestMethod]
	public async Task UpdateStockItemAsync_AmountChangedAndTransactionInsertFails_ThrowsAndKeepsStockItem()
	{
		// Arrange
		_database.Setup(d => d.GetOneAsync(It.IsAny<Expression<Func<StockItem, bool>>>())).ReturnsAsync(new StockItem { Code = "A1", Amount = 5 });
		var provider = new StockItemServiceProvider(_database.Object);

		// Act + Assert
		await Assert.ThrowsExceptionAsync<TimeoutException>(() => provider.UpdateStockItemAsync(new StockItem { Code = "A1", Amount = 3 }));
		_stockItems.Verify(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<StockItem>>(), It.IsAny<StockItem>(), It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Never);
	}
}
