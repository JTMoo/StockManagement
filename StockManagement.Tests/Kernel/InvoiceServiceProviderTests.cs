using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using StockManagement.Kernel.Database;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class InvoiceServiceProviderTests
{
	[TestMethod]
	public async Task UpdateInvoiceAsync_ExistingInvoice_FiltersById()
	{
		// Assign
		var invoice = new Invoice { Id = ObjectId.GenerateNewId().ToString(), Number = 42 };
		FilterDefinition<Invoice> usedFilter = null;

		var collection = new Mock<IMongoCollection<Invoice>>();
		collection
			.Setup(c => c.ReplaceOneAsync(It.IsAny<FilterDefinition<Invoice>>(), invoice, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()))
			.Callback<FilterDefinition<Invoice>, Invoice, ReplaceOptions, CancellationToken>((filter, _, _, _) => usedFilter = filter)
			.ReturnsAsync(Mock.Of<ReplaceOneResult>());

		var database = new Mock<IDatabase>();
		database.Setup(d => d.ConnectToMongo<Invoice>()).Returns(collection.Object);

		var provider = new InvoiceServiceProvider(database.Object);

		// Act
		await provider.UpdateInvoiceAsync(invoice);

		// Assert
		var serializer = BsonSerializer.SerializerRegistry.GetSerializer<Invoice>();
		var rendered = usedFilter.Render(new RenderArgs<Invoice>(serializer, BsonSerializer.SerializerRegistry));
		Assert.AreEqual(new BsonDocument("_id", ObjectId.Parse(invoice.Id)), rendered);
	}
}
