using System.Threading.Tasks;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class StockItemCreationDialogViewModel(IStockItemServiceProvider stockItemServiceProvider, StockItem stockItem) : DialogViewModelBase()
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	#region Properties
	public StockItem StockItem { get; set; } = stockItem;
	#endregion

	public override async void Confirm()
	{
		await CreateOrUpdateStockItem();
		base.Confirm();
	}

	private async Task CreateOrUpdateStockItem()
	{
		var stockItem = await _stockItemServiceProvider.GetStockItemAsync(this.StockItem.Code);
		if (stockItem == null)
		{
			await _stockItemServiceProvider.AddStockItemAsync(this.StockItem);
		}
		else
		{
			var result = await _stockItemServiceProvider.UpdateStockItemAsync(this.StockItem).ContinueWith(task => task.Result.ModifiedCount == 1);
		}
	}
}