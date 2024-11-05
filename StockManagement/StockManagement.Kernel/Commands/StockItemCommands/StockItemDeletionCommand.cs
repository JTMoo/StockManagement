using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemDeletionCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data.Value is not StockItem item) return false;

		item.Deregister();
		return true;
	}
}
