using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using System;
using System.Security.Cryptography.Xml;
using System.Windows;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel
{
    public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		CreateStockItemCommand = new RelayCommand<string>(this.OnCreateStockItemCommand);
    }

	public RelayCommand<string> QuitCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }

	private void OnCreateStockItemCommand(string obj)
	{
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Type = StockItemCommand.Type.SparePart
			}
		};
		MainManagerFacade.PushCommand(command);
	}
}
