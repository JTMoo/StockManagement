using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemDeletionCommand : ICommand
{
	public CommandData Data { get; set; }

	public Task<bool> Execute()
	{
		return Task.Factory.StartNew(this.DeregisterStockItem);
	}

	public bool DeregisterStockItem()
	{
		if (Data.Value is not StockItem item) return false;

		item.Deregister();
		return true;
	}
}
