using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands.Data;
using StockManagement.Kernel.Commands.StockItem;
using StockManagement.Kernel.Model;
using System;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class MoreInfoDialogViewModel : DialogViewModelBase
{
	private string _checkoutAmount;
	private string _checkinAmount;


	public MoreInfoDialogViewModel(StockItem stockItem) 
	{
		this.StockItem = stockItem;

		this.CheckoutCommand = new RelayCommand<string>(this.OnCheckoutCommand);
		this.CheckinCommand = new RelayCommand<string>(this.OnCheckinCommand);
	}

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
				StockItem = this.StockItem,
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
				StockItem = this.StockItem,
				Value = amount
			}
		};

		MainManagerFacade.PushCommand(command);
		base.Confirm(string.Empty);
	}
}