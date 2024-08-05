namespace StockManagement.Kernel.Commands;


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