using System.Threading.Tasks;
using StockManagement.Kernel.Database.Interfaces;
using StockManagement.Kernel.Model;

namespace StockManagement.Gui.ViewModel.Dialogs;


public class StockItemCreationDialogViewModel(IStockItemServiceProvider stockItemServiceProvider) : DialogViewModelBase()
{
	private readonly IStockItemServiceProvider _stockItemServiceProvider = stockItemServiceProvider;


	#region Properties
	public StockItem StockItem { get; set; }
	#endregion

	public override async void Confirm(string obj)
	{
		await CreateOrUpdateStockItem();
		base.Confirm(obj);
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