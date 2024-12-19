using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;


internal static class UniquePropertyHelper
{
	public static IList<CreateIndexModel<StockItem>> GetStockItemUniqueProperties()
	{
		IList<CreateIndexModel<StockItem>> creationModels = [];

		var options = new CreateIndexOptions() { Unique = true };
		List<StringFieldDefinition<StockItem>> uniqueProperties =
		[
			new StringFieldDefinition<StockItem>(nameof(StockItem.Code))
		];

		uniqueProperties.ForEach(property =>
		{
			var indexDefinition = new IndexKeysDefinitionBuilder<StockItem>().Ascending(property);
			var creationModel = new CreateIndexModel<StockItem>(indexDefinition, options);
			creationModels.Add(creationModel);
		});

		return creationModels;
	}

	public static IList<CreateIndexModel<Customer>> GetCustomerUniqueProperties()
	{
		IList<CreateIndexModel<Customer>> creationModels = [];

		var options = new CreateIndexOptions() { Unique = true };
		List<StringFieldDefinition<Customer>> uniqueProperties =
		[
			new StringFieldDefinition<Customer>(nameof(Customer.CustomerId))
		];

		uniqueProperties.ForEach(property =>
		{
			var indexDefinition = new IndexKeysDefinitionBuilder<Customer>().Ascending(property);
			var creationModel = new CreateIndexModel<Customer>(indexDefinition, options);
			creationModels.Add(creationModel);
		});

		return creationModels;
	}

	public static IList<CreateIndexModel<Invoice>> GetInvoiceUniqueProperties()
	{
		IList<CreateIndexModel<Invoice>> creationModels = [];

		var options = new CreateIndexOptions() { Unique = true };
		List<StringFieldDefinition<Invoice>> uniqueProperties =
		[
			new StringFieldDefinition<Invoice>(nameof(Invoice.Number))
		];

		uniqueProperties.ForEach(property =>
		{
			var indexDefinition = new IndexKeysDefinitionBuilder<Invoice>().Ascending(property);
			var creationModel = new CreateIndexModel<Invoice>(indexDefinition, options);
			creationModels.Add(creationModel);
		});

		return creationModels;
	}
}
