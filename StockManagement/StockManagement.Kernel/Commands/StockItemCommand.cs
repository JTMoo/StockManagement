using StockManagement.Kernel.Model;

namespace StockManagement.Kernel.Commands;


public class StockItemCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data.Value == null) return false;

		(Data.Value as StockItem)?.Register();

		return true;
	}
}