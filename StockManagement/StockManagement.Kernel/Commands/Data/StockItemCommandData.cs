using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands;


public class StockItemCommandData : CommandData
{
	public StockItem StockItem { get; set; }
}
