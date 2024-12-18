using MongoDB.Driver;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Database;
internal static class UniquePropertyHelper
{
	public static List<CreateIndexModel<StockItem>> GetStockItemUniqueProperties()
	{
		var creationModels = new List<CreateIndexModel<StockItem>>();

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
}
