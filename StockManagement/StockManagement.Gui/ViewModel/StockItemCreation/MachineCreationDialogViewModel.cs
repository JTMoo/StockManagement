using StockManagement.Kernel.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.StockItemCreation;

public class MachineCreationDialogViewModel : DialogViewModelBase
{
	public MachineCreationDialogViewModel() : base()
	{
	}


	public override void Confirm(string obj)
	{
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = new Machine(),
				Callback = success =>
				{
					if (success)
					{
						lock (GuiManager.Instance.MainViewModel.StockItemsLock)
						{
							GuiManager.Instance.MainViewModel.StockItems.Add(new Machine());
						}
					}
				}
			}
		};
		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}
