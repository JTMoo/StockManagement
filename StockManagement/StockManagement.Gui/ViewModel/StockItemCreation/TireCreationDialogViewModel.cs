using StockManagement.Kernel.Commands;
using StockManagement.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.StockItemCreation;

public class TireCreationDialogViewModel : DialogViewModelBase
{
	public TireCreationDialogViewModel() : base()
	{ }

	public override void Confirm(string obj)
	{
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Value = new Tire(),
				Callback = success =>
				{
					if (success)
					{
						lock (GuiManager.Instance.MainViewModel.StockItemsLock)
						{
							GuiManager.Instance.MainViewModel.StockItems.Add(new Tire());
						}
					}
				}
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(obj);
	}
}
