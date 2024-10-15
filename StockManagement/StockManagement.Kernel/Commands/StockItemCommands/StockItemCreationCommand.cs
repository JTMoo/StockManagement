using System.Diagnostics;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Model;
using StockManagement.Kernel.Model.ExtensionMethods;

namespace StockManagement.Kernel.Commands.StockItemCommands;


public class StockItemCreationCommand : ICommand
{
	public CommandData Data { get; set; }

	public bool Execute()
	{
		if (Data == null) return false;
		if (Data is not StockItemCommandData data)
		{
			Trace.WriteLine($"{nameof(StockItemCreationCommand)}: {nameof(StockItemCommandData)} expected..");
			return false;
		}

		switch(data.Type)
		{
			case StockItemCommandData.CreationCommandType.Single:
				if (data.DataToRegister is not StockItem item) return false;
				item.Register();
				break;
			case StockItemCommandData.CreationCommandType.Multiple:
				if (data.DataToRegister is not IEnumerable<StockItem> items) return false;
				items.Register();
				break;
			default:
				return false;
		}

		return true;
	}
}