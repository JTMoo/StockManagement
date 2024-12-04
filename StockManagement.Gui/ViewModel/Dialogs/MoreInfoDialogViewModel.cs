using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Commands.StockItemCommands;
using StockManagement.Kernel.Model;
using System;
using static MongoDB.Driver.WriteConcern;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class MoreInfoDialogViewModel : DialogViewModelBase
{
	private string _checkoutAmount;
	private string _checkinAmount;


	public MoreInfoDialogViewModel(StockItem stockItem) 
	{
		this.StockItem = stockItem;

		this.DeleteItemCommand = new RelayCommand<string>(this.OnDeleteItemCommand);
		this.CheckoutCommand = new RelayCommand<string>(this.OnCheckoutCommand);
		this.CheckinCommand = new RelayCommand<string>(this.OnCheckinCommand);
	}


	#region Properties
	public RelayCommand<string> DeleteItemCommand { get; }
	public RelayCommand<string> CheckoutCommand { get; }
	public RelayCommand<string> CheckinCommand { get; }

	public StockItem StockItem { get; }

	public string CheckoutAmount
	{
		get { return this._checkoutAmount; }
		set { this.SetField(ref this._checkoutAmount, value); }
	}

	public string CheckinAmount
	{
		get { return this._checkinAmount; }
		set { this.SetField(ref this._checkinAmount, value); }
	}
	#endregion Properties


	private void OnDeleteItemCommand(string obj)
	{
		var command = new StockItemDeletionCommand()
		{
			Data = new StockItemCommandData()
			{
				Value = this.StockItem,
				Callback = Close
			}
		};

		MainManagerFacade.PushCommand(command);
	}

	private void OnCheckoutCommand(string param)
	{
		if (string.IsNullOrEmpty(this.CheckoutAmount)) return;

		var tryParse = int.TryParse(this.CheckoutAmount, out var amount);
		if (!tryParse) return;

		var command = new StockItemChangeAmountCommand()
		{
			SelectedChangeType = StockItemChangeAmountCommand.ChangeType.Checkout,
			Data = new StockItemCommandData()
			{
				DataToRegister = this.StockItem,
				Value = amount
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(string.Empty);
	}

	private void OnCheckinCommand(string param)
	{
		if (string.IsNullOrEmpty(this.CheckinAmount)) return;

		var tryParse = Int32.TryParse(this.CheckinAmount, out var amount);
		if (!tryParse) return;

		var command = new StockItemChangeAmountCommand()
		{
			SelectedChangeType = StockItemChangeAmountCommand.ChangeType.Checkin,
			Data = new StockItemCommandData()
			{
				DataToRegister = this.StockItem,
				Value = amount
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(string.Empty);
	}
}