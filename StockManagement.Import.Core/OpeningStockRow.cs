using System.ComponentModel.DataAnnotations;

namespace StockManagement.Import.Core;


/// <summary>
/// One opening-stock import row: units to check into an existing <see cref="Kernel.Model.StockItem"/>, matched by <see cref="Code"/>
/// </summary>
internal sealed class OpeningStockRow
{
	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.code))]
	public string Code { get; set; } = string.Empty;

	[Display(ResourceType = typeof(Language.StockItems), Name = nameof(Language.StockItems.amount))]
	public int Amount { get; set; }
}
