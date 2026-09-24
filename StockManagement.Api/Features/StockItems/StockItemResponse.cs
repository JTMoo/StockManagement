using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.Types;

namespace StockManagement.Api.Features.StockItems;


/// <summary>
/// Article with its units in stock
/// </summary>
public sealed record StockItemResponse(string Code, string Name, string Description, string Location, int Amount, decimal Price, ManufacturerType Manufacturer)
{
	public static StockItemResponse From(StockItem stockItem)
	{
		return new(stockItem.Code, stockItem.Name, stockItem.Description, stockItem.Location, stockItem.Amount, (decimal)stockItem.Price, stockItem.Manufacturer);
	}
}
