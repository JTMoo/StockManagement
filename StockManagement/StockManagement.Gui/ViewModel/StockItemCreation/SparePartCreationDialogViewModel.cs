using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.StockItemCreation;


public class SparePartCreationDialogViewModel : DialogViewModelBase
{
	public SparePartCreationDialogViewModel() : base()
	{ }

	public override void Confirm(string obj)
	{
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = new SparePart(),
				Callback = success =>
				{
					if (success)
					{
						lock (GuiManager.Instance.MainViewModel.StockItemsLock)
						{
							GuiManager.Instance.MainViewModel.StockItems.Add(new SparePart());
						}
					}
				}
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}
