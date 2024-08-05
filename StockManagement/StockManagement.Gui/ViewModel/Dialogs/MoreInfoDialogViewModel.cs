using MongoDB.Driver;
using StockManagement.Gui.Commands;
using StockManagement.Kernel;
using StockManagement.Kernel.Commands;
using StockManagement.Kernel.Model;
using System;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class MoreInfoDialogViewModel : DialogViewModelBase
{
	private string checkoutAmount;
	private string checkinAmount;


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
		get { return this.checkoutAmount; }
		set { this.SetField(ref this.checkoutAmount, value); }
	}

	public string CheckinAmount
	{
		get { return this.checkinAmount; }
		set { this.SetField(ref this.checkinAmount, value); }
	}


	private void OnCheckoutCommand(string param)
	{
		if (this.CheckoutAmount == null || this.CheckoutAmount == "") return;

		int amount;
		Int32.TryParse(this.CheckoutAmount, out amount);
		if (amount == 0) return;

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
		if (this.CheckinAmount == null || this.CheckinAmount == "") return;

		int amount;
		Int32.TryParse(this.CheckinAmount, out amount);
		if (amount == 0) return;

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
