using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel;


public static class MainManagerFacade
{
	public static bool PushCommand(ICommand command)
	{
		return MainManager.Instance.PushCommand(command);
	}

	public static List<StockItem> GetStockItems()
	{
		var stockItems = new List<StockItem>();
		stockItems.AddRange(MainManager.Instance.MachineManager.Machines);
		stockItems.AddRange(MainManager.Instance.SparePartManager.SpareParts);
		stockItems.AddRange(MainManager.Instance.TireManager.Tires);

		return stockItems;
	}
}
