using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using System;
using System.Security.Cryptography.Xml;
using System.Windows;

namespace StockManagement.Gui.ViewModel;


internal class MainViewModel : NotificationBase
{
	private bool _showDialog = false;


    public MainViewModel()
    {
        QuitCommand = new RelayCommand<string>(_ => Application.Current.Shutdown());
		ConfirmDialogCommand = new RelayCommand<string>(this.OnConfirmDialogCommand);
		CancelDialogCommand = new RelayCommand<string>(_ => this.ShowDialog = false);
		CreateStockItemCommand = new RelayCommand<string>(_ => this.ShowDialog = true);
    }

	public RelayCommand<string> QuitCommand { get; }
	public RelayCommand<string> ConfirmDialogCommand { get; }
	public RelayCommand<string> CancelDialogCommand { get; }
    public RelayCommand<string> CreateStockItemCommand { get; }

	public bool ShowDialog
	{
		get { return _showDialog; }
		private set { this.SetField(ref _showDialog, value); }
	}

	private void OnConfirmDialogCommand(string obj)
	{
		var command = new StockItemCommand
		{
			Data = new CommandData
			{
				Type = StockItemCommand.Type.SparePart
			}
		};
		MainManagerFacade.PushCommand(command);
		this.ShowDialog = false;
	}
}
