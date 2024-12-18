using StockManagement.Gui.Commands;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class MoreInfoDialogViewModel : DialogViewModelBase
{
	private string _checkoutAmount;
	private string _checkinAmount;
	private readonly IStockItemServiceProvider _stockItemServiceProvider;


	public MoreInfoDialogViewModel(StockItem stockItem, IStockItemServiceProvider stockItemServiceProvider) 
	{
		_stockItemServiceProvider = stockItemServiceProvider;
		this.StockItem = stockItem;

		this.CheckoutCommand = new RelayCommand<string>(this.OnCheckoutCommand);
		this.CheckinCommand = new RelayCommand<string>(this.OnCheckinCommand);
	}


	#region Properties
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


	private async void OnCheckoutCommand(string param)
	{
		if (string.IsNullOrEmpty(this.CheckoutAmount)) return;
		if (!int.TryParse(this.CheckoutAmount, out var amount)) return;

		this.StockItem.Amount -= amount;
		await _stockItemServiceProvider.UpdateStockItemAsync(this.StockItem);
		base.Confirm(string.Empty);
	}

	private async void OnCheckinCommand(string param)
	{
		if (string.IsNullOrEmpty(this.CheckinAmount)) return;
		if (!int.TryParse(this.CheckinAmount, out var amount)) return;

		this.StockItem.Amount += amount;
		await _stockItemServiceProvider.UpdateStockItemAsync(this.StockItem);
		base.Confirm(string.Empty);
	}
}