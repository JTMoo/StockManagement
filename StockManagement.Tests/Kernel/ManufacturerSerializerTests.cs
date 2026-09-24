using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using StockManagement.Kernel.Model;

namespace StockManagement.Tests.Kernel;


[TestClass]
public sealed class ManufacturerSerializerTests
{
	[TestMethod]
	public void Deserialize_LegacyEnumNumber_ReturnsName()
	{
		// Arrange
		var document = new BsonDocument { { "Code", "A1" }, { "Manufacturer", 2 } };

		// Act
		var stockItem = BsonSerializer.Deserialize<StockItem>(document);

		// Assert
		Assert.AreEqual("Metallfach", stockItem.Manufacturer);
	}

	[TestMethod]
	public void Deserialize_LegacyNone_ReturnsEmpty()
	{
		// Arrange
		var document = new BsonDocument { { "Code", "A1" }, { "Manufacturer", 0 } };

		// Act
		var stockItem = BsonSerializer.Deserialize<StockItem>(document);

		// Assert
		Assert.AreEqual(string.Empty, stockItem.Manufacturer);
	}

	[TestMethod]
	public void Deserialize_UnknownNumber_ReturnsEmpty()
	{
		// Arrange
		var document = new BsonDocument { { "Code", "A1" }, { "Manufacturer", 99 } };

		// Act
		var stockItem = BsonSerializer.Deserialize<StockItem>(document);

		// Assert
		Assert.AreEqual(string.Empty, stockItem.Manufacturer);
	}

	[TestMethod]
	public void Deserialize_Name_ReturnsName()
	{
		// Arrange
		var document = new BsonDocument { { "Code", "A1" }, { "Manufacturer", "Kuhn" } };

		// Act
		var stockItem = BsonSerializer.Deserialize<StockItem>(document);

		// Assert
		Assert.AreEqual("Kuhn", stockItem.Manufacturer);
	}

	[TestMethod]
	public void Serialize_Name_WritesString()
	{
		// Arrange
		var stockItem = new StockItem("Blade", code: "A1", manufacturer: "Kuhn");

		// Act
		var document = stockItem.ToBsonDocument();

		// Assert
		Assert.AreEqual(new BsonString("Kuhn"), document["Manufacturer"]);
	}
}
