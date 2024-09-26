using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemCreationCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data is not StockItemCommandData data || data.StockItem is not StockItem item) return false;

		item.Register();

		return true;
	}
}