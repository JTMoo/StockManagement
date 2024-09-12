using StockManagement.Kernel.Commands.Data;

namespace StockManagement.Kernel.Commands.StockItem;


public class StockItemCreationCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data is not StockItemCommandData) return false;


		((StockItemCommandData)Data).StockItem?.Register();

		return true;
	}
}